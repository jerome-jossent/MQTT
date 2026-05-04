import paho.mqtt.client as mqtt
import numpy as np
import matplotlib.pyplot as plt
import matplotlib.animation as animation
import matplotlib.widgets as widgets
import threading
import time
import json
import os
from collections import deque

# ══════════════════════════════════════════════════════════
# POLICE
# ══════════════════════════════════════════════════════════
plt.rcParams['font.family'] = 'DejaVu Sans'

# ══════════════════════════════════════════════════════════
# CONFIGURATION
# ══════════════════════════════════════════════════════════
MQTT_BROKER         = "192.168.0.242"
MQTT_PORT           = 1883
MQTT_TOPIC          = "lidar/raw"
MQTT_TOPIC_DIFF     = "lidar/diff"
BASELINE_WINDOW_SEC = 2.0
BASELINE_RESOLUTION = 1.0
TEMPORAL_WINDOW_SEC = 10.0
SAVE_FILE           = "lidar_config.json"
MQTT_PUBLISH_HZ     = 2.0

# ══════════════════════════════════════════════════════════
# ETAT GLOBAL
# ══════════════════════════════════════════════════════════
latest_angles    = np.array([])
latest_distances = np.array([])
data_lock        = threading.Lock()
new_data_flag    = False

angle_filter = {"start": 0.0, "end": 360.0, "enabled": False}

baseline_buffer    = deque()
baseline_data      = None
baseline_lock      = threading.Lock()
baseline_active    = False
capturing          = False
capture_start_time = [None]

temporal_buffer = deque()
temporal_lock   = threading.Lock()

diff_max_observed = [100.0]

graph_enabled = {
    "radar":    True,
    "diff_xy":  True,
    "temporal": True,
}

last_diff_mean  = [None]
last_diff_std   = [None]
last_diff_n     = [0]
last_publish_t  = [0.0]
mqtt_publish_client = [None]

# ══════════════════════════════════════════════════════════
# PERSISTANCE
# ══════════════════════════════════════════════════════════
def save_config():
    config = {
        "angle_filter": {
            "start":   angle_filter["start"],
            "end":     angle_filter["end"],
            "enabled": angle_filter["enabled"]
        },
        "baseline": None
    }
    if baseline_data is not None:
        config["baseline"] = {
            "angles_rad": baseline_data[:, 0].tolist(),
            "distances":  baseline_data[:, 1].tolist(),
            "saved_at":   time.strftime('%Y-%m-%d %H:%M:%S')
        }
    with open(SAVE_FILE, 'w') as f:
        json.dump(config, f, indent=2)
    print(f"Config sauvegardee -> {SAVE_FILE}")

def load_config():
    global baseline_data
    if not os.path.exists(SAVE_FILE):
        print("Pas de config sauvegardee.")
        return False
    try:
        with open(SAVE_FILE, 'r') as f:
            config = json.load(f)
        af = config.get("angle_filter", {})
        angle_filter["start"]   = float(af.get("start",   0.0))
        angle_filter["end"]     = float(af.get("end",   360.0))
        angle_filter["enabled"] = bool(af.get("enabled", False))
        bl = config.get("baseline")
        if bl is not None:
            angles = np.array(bl["angles_rad"], dtype=np.float32)
            dists  = np.array(bl["distances"],  dtype=np.float32)
            baseline_data = np.column_stack([angles, dists])
            print(f"Baseline chargee : {len(baseline_data)} pts "
                  f"({bl.get('saved_at', '?')})")
            return True
        return False
    except Exception as e:
        print(f"Erreur chargement : {e}")
        return False

# ══════════════════════════════════════════════════════════
# MQTT
# ══════════════════════════════════════════════════════════
def on_message(client, userdata, msg):
    global latest_angles, latest_distances, new_data_flag
    payload = msg.payload
    if len(payload) % 4 != 0:
        return
    n_points   = len(payload) // 4
    data       = np.frombuffer(payload, dtype=np.uint16).reshape(n_points, 2)
    angles_deg = data[:, 0].astype(np.float32) / 100.0
    distances  = data[:, 1].astype(np.float32)
    with data_lock:
        latest_angles    = np.deg2rad(angles_deg)
        latest_distances = distances
        new_data_flag    = True
    if capturing:
        now = time.time()
        with baseline_lock:
            baseline_buffer.append((now, angles_deg.copy(), distances.copy()))
            cutoff = now - BASELINE_WINDOW_SEC
            while baseline_buffer and baseline_buffer[0][0] < cutoff:
                baseline_buffer.popleft()

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("MQTT connecte")
        client.subscribe(MQTT_TOPIC)
    else:
        print(f"Connexion echouee rc={rc}")

def publish_diff(mean_mm, std_mm, n_pts):
    client = mqtt_publish_client[0]
    if client is None:
        return
    now = time.time()
    if now - last_publish_t[0] < 1.0 / MQTT_PUBLISH_HZ:
        return
    last_publish_t[0] = now
    # payload = json.dumps({
    #     "mean_mm": round(float(mean_mm), 2),
    #     "std_mm":  round(float(std_mm),  2),
    #     "n":       int(n_pts),
    #     "ts":      round(now, 3)
    # })
    payload = round(float(mean_mm), 2)
    try:
        client.publish(MQTT_TOPIC_DIFF, payload, qos=0, retain=False)
    except Exception as e:
        print(f"[MQTT publish] erreur : {e}")

def start_mqtt():
    mqttc = mqtt.Client()
    mqttc.on_connect = on_connect
    mqttc.on_message = on_message
    mqttc.connect(MQTT_BROKER, MQTT_PORT, 60)
    mqtt_publish_client[0] = mqttc
    mqttc.loop_forever()

threading.Thread(target=start_mqtt, daemon=True).start()

# ══════════════════════════════════════════════════════════
# FILTRES & CALCULS
# ══════════════════════════════════════════════════════════
def _angle_mask_deg(angles_deg):
    s = angle_filter["start"] % 360
    e = angle_filter["end"]   % 360
    a = angles_deg % 360
    mask_in = (a >= s) & (a <= e) if s <= e else (a >= s) | (a <= e)
    return mask_in, ~mask_in

def apply_angle_filter(angles_rad):
    if not angle_filter["enabled"]:
        return np.ones(len(angles_rad), dtype=bool)
    return _angle_mask_deg(np.rad2deg(angles_rad) % 360)[0]

def compute_baseline():
    with baseline_lock:
        if not baseline_buffer:
            return None
        all_ang, all_dst = [], []
        for (_, ang_deg, dists) in baseline_buffer:
            valid = (dists > 50) & (dists < 12000)
            a, d  = ang_deg[valid], dists[valid]
            if angle_filter["enabled"]:
                mask, _ = _angle_mask_deg(a)
                a, d = a[mask], d[mask]
            all_ang.append(a)
            all_dst.append(d)
    if not all_ang:
        return None
    all_ang = np.concatenate(all_ang)
    all_dst = np.concatenate(all_dst)
    bins    = np.arange(0, 360 + BASELINE_RESOLUTION, BASELINE_RESOLUTION)
    res_a, res_d = [], []
    for lo, hi in zip(bins[:-1], bins[1:]):
        mask = (all_ang >= lo) & (all_ang < hi)
        if mask.sum() > 0:
            res_a.append((lo + hi) / 2.0)
            res_d.append(np.mean(all_dst[mask]))
    if not res_a:
        return None
    return np.column_stack([
        np.deg2rad(np.array(res_a, dtype=np.float32)),
        np.array(res_d, dtype=np.float32)
    ])

def compute_diff(angles_rad, distances):
    with baseline_lock:
        bl = baseline_data
    if bl is None or len(bl) == 0:
        return None, None
    mask  = apply_angle_filter(angles_rad)
    valid = (distances > 50) & (distances < 12000) & mask
    a_live = angles_rad[valid]
    d_live = distances[valid]
    if len(a_live) == 0:
        return None, None
    idx       = np.argsort(bl[:, 0])
    bl_interp = np.interp(a_live, bl[idx, 0], bl[idx, 1],
                          left=np.nan, right=np.nan)
    diff         = d_live - bl_interp
    valid_interp = ~np.isnan(diff)
    return a_live[valid_interp], diff[valid_interp]

def angles_to_display(angles_rad):
    angles_deg = np.rad2deg(angles_rad) % 360
    s = angle_filter["start"] % 360
    e = angle_filter["end"]   % 360
    if s > e:
        mask_neg = angles_deg >= s
        angles_deg[mask_neg] -= 360.0
    return angles_deg

# ══════════════════════════════════════════════════════════
# FIGURE  —  layout général
# ══════════════════════════════════════════════════════════
# Fenêtre large, hauteur suffisante
fig = plt.figure(figsize=(20, 10), facecolor='black')
fig.patch.set_facecolor('black')

# Colonnes :  radar(0.00-0.34) | graphiques(0.36-0.64) | panneau(0.66-1.00)
# Le panneau fait 0.34 de large → beaucoup de place pour les contrôles

# ── Radar ─────────────────────────────────────────────────
ax_radar = fig.add_axes([0.01, 0.05, 0.32, 0.90],
                         projection='polar', facecolor='black')
ax_radar.set_theta_zero_location('N')
ax_radar.set_theta_direction(-1)
ax_radar.set_ylim(0, 5000)
ax_radar.tick_params(colors='lime')
ax_radar.spines['polar'].set_color('lime')
ax_radar.grid(color='lime', linestyle='--', linewidth=0.3, alpha=0.4)
ax_radar.set_yticks([1000, 2000, 3000, 4000, 5000])
ax_radar.set_yticklabels(['1m','2m','3m','4m','5m'], color='lime', fontsize=7)
title_radar = ax_radar.set_title("LD06 Lidar - En attente...",
                                  color='lime', fontsize=9, pad=12)

# ── Diff XY ───────────────────────────────────────────────
ax_diff = fig.add_axes([0.36, 0.53, 0.28, 0.42], facecolor='#0a0a0a')
ax_diff.tick_params(colors='#888888', labelsize=7)
ax_diff.spines[:].set_color('#555555')
ax_diff.set_xlabel("Angle (deg)", color='#888888', fontsize=8)
ax_diff.set_ylabel("diff distance (mm)", color='#888888', fontsize=8)
ax_diff.set_title("Difference Live - Baseline", color='white', fontsize=9)
ax_diff.axhline(0, color='white', linewidth=0.8, linestyle='--', alpha=0.5)
ax_diff.grid(color='#333333', linestyle='--', linewidth=0.3, alpha=0.6)

# ── Temporel ──────────────────────────────────────────────
ax_time = fig.add_axes([0.36, 0.07, 0.28, 0.37], facecolor='#0a0a0a')
ax_time.tick_params(colors='#888888', labelsize=7)
ax_time.spines[:].set_color('#555555')
ax_time.set_xlabel("Temps (s)", color='#888888', fontsize=8)
ax_time.set_ylabel("diff moyen (mm)", color='#888888', fontsize=8)
ax_time.set_title("Moyenne diff - 10 dernieres secondes",
                   color='white', fontsize=9)
ax_time.set_xlim(-TEMPORAL_WINDOW_SEC, 0)
ax_time.axhline(0, color='white', linewidth=0.8, linestyle='--', alpha=0.5)
ax_time.grid(color='#333333', linestyle='--', linewidth=0.3, alpha=0.6)

# ══════════════════════════════════════════════════════════
# SERIES DE DONNEES
# ══════════════════════════════════════════════════════════
scatter_in  = ax_radar.scatter([], [], s=2, c='lime',  alpha=0.8,  zorder=3)
scatter_out = ax_radar.scatter([], [], s=1, c='gray',  alpha=0.15, zorder=2)
baseline_line, = ax_radar.plot([], [], 'o', color='white',
                                markersize=2, alpha=0.7, zorder=5)
baseline_line.set_visible(False)
line_start = ax_radar.axvline(x=np.deg2rad(angle_filter["start"]),
    color='cyan',   linewidth=1.5, linestyle='--', alpha=0.3, zorder=6)
line_end   = ax_radar.axvline(x=np.deg2rad(angle_filter["end"]),
    color='orange', linewidth=1.5, linestyle='--', alpha=0.3, zorder=6)

diff_scatter = ax_diff.scatter([], [], s=4, c=[], cmap='RdBu_r',
                                vmin=-500, vmax=500, alpha=0.8, zorder=3)
diff_trend, = ax_diff.plot([], [], color='yellow', linewidth=1.2,
                            alpha=0.8, zorder=4, label='Tendance')
ax_diff.legend(loc='upper right', fontsize=6,
               facecolor='#111111', labelcolor='white', framealpha=0.6)
diff_fill_pos = [None]
diff_fill_neg = [None]

time_line, = ax_time.plot([], [], color='yellow', linewidth=1.5, zorder=3)
time_dot,  = ax_time.plot([], [], 'o', color='yellow', markersize=5, zorder=4)
time_fill_storage = [None]
band_fill_storage = [None]

# ══════════════════════════════════════════════════════════
# PANNEAU DE CONTROLE  —  colonne droite  x=0.67..0.99
# ══════════════════════════════════════════════════════════
# On découpe la hauteur (0..1) en blocs :
#
#  0.96 ┬ Titre filtre angulaire
#  0.93 ┤ Slider début
#  0.88 ┤
#  0.84 ┤ Slider fin
#  0.79 ┤
#  0.75 ┤ Saisie directe (2 textbox)
#  0.70 ┤ Boutons toggle/reset filtre
#  0.65 ┤ Info filtre
#  0.62 ┼─ séparateur
#  0.59 ┤ Titre baseline
#  0.56 ┤ Barre progression
#  0.52 ┤ Boutons capturer / toggle baseline
#  0.47 ┤ Bouton effacer baseline
#  0.43 ┤ Info baseline
#  0.40 ┼─ séparateur
#  0.37 ┤ Stats diff actuel
#  0.33 ┤ Stats diff 10s
#  0.29 ┤ MQTT pub indicator
#  0.25 ┼─ séparateur
#  0.22 ┤ Titre cases à cocher
#  0.02 ┤ CheckButtons (3 cases)
#  0.00 ┘

PX  = 0.670   # bord gauche panneau
PW  = 0.155   # largeur demi-colonne
PW2 = 0.320   # largeur pleine colonne
PXR = 0.835   # bord gauche colonne droite du panneau

# ── Titre filtre ──────────────────────────────────────────
fig.text(PX, 0.965, "── Filtre Angulaire ──",
         color='cyan', fontsize=9, fontweight='bold')

# Slider début
ax_sl_s = fig.add_axes([PX, 0.900, PW2, 0.030], facecolor='#222222')
slider_start = widgets.Slider(ax_sl_s, '', 0, 360,
                               valinit=angle_filter["start"],
                               valstep=1, color='cyan')
ax_sl_s.set_title('Angle debut (deg)', color='cyan', fontsize=7, pad=2)
slider_start.valtext.set_color('cyan')
slider_start.valtext.set_fontsize(7)

# Slider fin
ax_sl_e = fig.add_axes([PX, 0.845, PW2, 0.030], facecolor='#222222')
slider_end = widgets.Slider(ax_sl_e, '', 0, 360,
                             valinit=angle_filter["end"],
                             valstep=1, color='orange')
ax_sl_e.set_title('Angle fin (deg)', color='orange', fontsize=7, pad=2)
slider_end.valtext.set_color('orange')
slider_end.valtext.set_fontsize(7)

# TextBox saisie directe
fig.text(PX, 0.818, "Saisie directe :", color='#aaaaaa', fontsize=7)
ax_txt_s = fig.add_axes([PX,  0.782, 0.130, 0.030], facecolor='#222222')
text_start = widgets.TextBox(ax_txt_s, 'Deb:',
                              initial=str(int(angle_filter["start"])),
                              color='#222222', hovercolor='#333333')
text_start.label.set_color('cyan')
text_start.label.set_fontsize(7)

ax_txt_e = fig.add_axes([PXR, 0.782, 0.130, 0.030], facecolor='#222222')
text_end = widgets.TextBox(ax_txt_e, 'Fin:',
                            initial=str(int(angle_filter["end"])),
                            color='#222222', hovercolor='#333333')
text_end.label.set_color('orange')
text_end.label.set_fontsize(7)

# Boutons toggle / reset filtre
ax_btn_tog = fig.add_axes([PX,  0.730, PW, 0.044], facecolor='#333333')
btn_toggle = widgets.Button(
    ax_btn_tog,
    '[ON]  Filtre' if angle_filter["enabled"] else '[OFF] Filtre',
    color='#004400' if angle_filter["enabled"] else '#333333',
    hovercolor='#555555')
btn_toggle.label.set_color('white')
btn_toggle.label.set_fontsize(7)

ax_btn_rst = fig.add_axes([PXR, 0.730, PW, 0.044], facecolor='#333333')
btn_reset = widgets.Button(ax_btn_rst, 'Reset filtre',
                            color='#333333', hovercolor='#555555')
btn_reset.label.set_color('white')
btn_reset.label.set_fontsize(7)

info_text = fig.text(PX, 0.710, "", color='yellow', fontsize=7)

# ── Séparateur 1 ──────────────────────────────────────────
fig.add_artist(plt.Line2D([0.662, 0.995], [0.698, 0.698],
               transform=fig.transFigure, color='#444444', linewidth=1))

# ── Baseline ──────────────────────────────────────────────
fig.text(PX, 0.682, "── Baseline ──",
         color='white', fontsize=9, fontweight='bold')
fig.text(PX, 0.663,
         f"Fenetre {BASELINE_WINDOW_SEC:.0f}s | Res {BASELINE_RESOLUTION}deg/bin",
         color='#777777', fontsize=6)

# Barre de progression
ax_prog = fig.add_axes([PX, 0.638, PW2, 0.020], facecolor='#222222')
ax_prog.set_xlim(0, 1); ax_prog.set_ylim(0, 1)
ax_prog.set_xticks([]); ax_prog.set_yticks([])
ax_prog.spines[:].set_color('#444444')
progress_bar   = ax_prog.barh(0.5, 0, height=0.8, color='white', alpha=0.8)[0]
progress_label = ax_prog.text(0.5, 0.5, 'Pret', ha='center', va='center',
                               color='black', fontsize=6, fontweight='bold')

# Boutons capturer / toggle baseline
ax_btn_cap = fig.add_axes([PX,  0.588, PW, 0.044], facecolor='#333333')
btn_capture = widgets.Button(ax_btn_cap, 'Capturer BL',
                              color='#333333', hovercolor='#555555')
btn_capture.label.set_color('white')
btn_capture.label.set_fontsize(7)

ax_btn_bl = fig.add_axes([PXR, 0.588, PW, 0.044], facecolor='#333333')
btn_baseline = widgets.Button(
    ax_btn_bl,
    '[ON]  Aff.BL' if baseline_active else '[OFF] Aff.BL',
    color='#003366' if baseline_active else '#333333',
    hovercolor='#555555')
btn_baseline.label.set_color('white')
btn_baseline.label.set_fontsize(7)

# Bouton effacer baseline
ax_btn_clr = fig.add_axes([PX + 0.080, 0.540, 0.160, 0.040],
                           facecolor='#333333')
btn_clear = widgets.Button(ax_btn_clr, 'Effacer Baseline',
                            color='#333333', hovercolor='#555555')
btn_clear.label.set_color('#ff6666')
btn_clear.label.set_fontsize(7)

baseline_info = fig.text(PX, 0.522, "Baseline : aucune",
                          color='#aaaaaa', fontsize=6)

# ── Séparateur 2 ──────────────────────────────────────────
fig.add_artist(plt.Line2D([0.662, 0.995], [0.510, 0.510],
               transform=fig.transFigure, color='#444444', linewidth=1))

# ── Stats différence ──────────────────────────────────────
diff_stats = fig.text(PX, 0.494,
                       "diff actuel : --",
                       color='yellow', fontsize=7)
diff_stats2 = fig.text(PX, 0.472,
                        "diff 10s : moy=-- | std=-- | min=-- | max=--",
                        color='#aaaaaa', fontsize=6)

# Bouton reset échelle diff
ax_btn_diff_rst = fig.add_axes([PX + 0.060, 0.430, 0.200, 0.036],
                                facecolor='#333333')
btn_diff_reset = widgets.Button(ax_btn_diff_rst, 'Reset echelle diff',
                                 color='#333333', hovercolor='#555555')
btn_diff_reset.label.set_color('#aaaaaa')
btn_diff_reset.label.set_fontsize(7)

# ── Séparateur 3 ──────────────────────────────────────────
fig.add_artist(plt.Line2D([0.662, 0.995], [0.418, 0.418],
               transform=fig.transFigure, color='#444444', linewidth=1))

# ── MQTT publish indicator ────────────────────────────────
fig.text(PX, 0.403, "── Publication MQTT ──",
         color='#aaaaaa', fontsize=8, fontstyle='italic')
mqtt_pub_indicator = fig.text(
    PX, 0.382,
    f"Topic : {MQTT_TOPIC_DIFF}",
    color='#555555', fontsize=6)
mqtt_pub_value = fig.text(
    PX, 0.363,
    "Valeur : --",
    color='#555555', fontsize=7)

# ── Séparateur 4 ──────────────────────────────────────────
fig.add_artist(plt.Line2D([0.662, 0.995], [0.350, 0.350],
               transform=fig.transFigure, color='#444444', linewidth=1))

# ── Cases à cocher ────────────────────────────────────────
fig.text(PX, 0.334, "── Mise a jour graphiques ──",
         color='#aaaaaa', fontsize=8, fontstyle='italic')

# Trois axes séparés, un par case → contournement complet
# des problèmes de version matplotlib
CHECK_Y   = [0.270, 0.210, 0.150]   # positions Y des 3 cases
CHECK_LBL = ['Radar', 'Diff XY', 'Temporel']
CHECK_KEY = ['radar', 'diff_xy', 'temporal']
CHECK_COL = ['lime', 'cyan', 'yellow']

btn_checks = []   # liste des Button widgets utilisés comme toggles

for i, (y, lbl, key, col) in enumerate(
        zip(CHECK_Y, CHECK_LBL, CHECK_KEY, CHECK_COL)):
    ax_c = fig.add_axes([PX, y, PW2, 0.048], facecolor='#1a1a1a')
    ax_c.spines[:].set_color('#444444')
    state = graph_enabled[key]
    face  = '#003300' if state else '#1a1a1a'
    label = f'[ON]  {lbl}' if state else f'[OFF] {lbl}'
    b = widgets.Button(ax_c, label, color=face, hovercolor='#333333')
    b.label.set_color(col)
    b.label.set_fontsize(8)
    btn_checks.append((b, ax_c, key, lbl, col))

def make_check_callback(idx):
    def _cb(event):
        b, ax_c, key, lbl, col = btn_checks[idx]
        graph_enabled[key] = not graph_enabled[key]
        state = graph_enabled[key]
        b.label.set_text(f'[ON]  {lbl}' if state else f'[OFF] {lbl}')
        ax_c.set_facecolor('#003300' if state else '#1a1a1a')
        fig.canvas.draw_idle()
        print(f"[Graph] {lbl} -> {'ON' if state else 'OFF'}")
    return _cb

for i, (b, ax_c, key, lbl, col) in enumerate(btn_checks):
    b.on_clicked(make_check_callback(i))

# ══════════════════════════════════════════════════════════
# FONCTIONS STATIQUES
# ══════════════════════════════════════════════════════════
def redraw_angle_lines():
    s_rad = np.deg2rad(angle_filter["start"])
    e_rad = np.deg2rad(angle_filter["end"])
    line_start.set_xdata([s_rad, s_rad])
    line_end.set_xdata([e_rad, e_rad])
    alpha = 0.9 if angle_filter["enabled"] else 0.3
    line_start.set_alpha(alpha)
    line_end.set_alpha(alpha)
    s, e  = angle_filter["start"], angle_filter["end"]
    span  = (e - s) % 360 or 360
    state = "ON" if angle_filter["enabled"] else "OFF"
    info_text.set_text(
        f"Zone : {s:.0f}→{e:.0f} deg ({span:.0f}°)  [Filtre: {state}]")
    fig.canvas.draw_idle()
    save_config()

def redraw_baseline():
    with baseline_lock:
        bl = baseline_data
    if bl is not None and len(bl) > 0:
        idx = np.argsort(bl[:, 0])
        baseline_line.set_data(bl[idx, 0], bl[idx, 1])
        baseline_line.set_visible(baseline_active)
        baseline_info.set_text(
            f"BL : {len(bl)} pts | "
            f"moy {bl[:,1].mean():.0f}mm | "
            f"min {bl[:,1].min():.0f}mm | "
            f"max {bl[:,1].max():.0f}mm")
    else:
        baseline_line.set_data([], [])
        baseline_line.set_visible(False)
        baseline_info.set_text("Baseline : aucune")
    fig.canvas.draw_idle()

# ══════════════════════════════════════════════════════════
# CALLBACKS
# ══════════════════════════════════════════════════════════
def on_slider_start(val):
    angle_filter["start"] = float(val)
    text_start.set_val(str(int(val)))
    redraw_angle_lines()

def on_slider_end(val):
    angle_filter["end"] = float(val)
    text_end.set_val(str(int(val)))
    redraw_angle_lines()

def on_text_start(text):
    try:
        val = float(text) % 360
        angle_filter["start"] = val
        slider_start.set_val(val)
        redraw_angle_lines()
    except ValueError:
        pass

def on_text_end(text):
    try:
        val = float(text) % 360
        angle_filter["end"] = val
        slider_end.set_val(val)
        redraw_angle_lines()
    except ValueError:
        pass

def on_toggle(event):
    angle_filter["enabled"] = not angle_filter["enabled"]
    if angle_filter["enabled"]:
        btn_toggle.label.set_text('[ON]  Filtre')
        ax_btn_tog.set_facecolor('#004400')
    else:
        btn_toggle.label.set_text('[OFF] Filtre')
        ax_btn_tog.set_facecolor('#333333')
    redraw_angle_lines()

def on_reset(event):
    angle_filter.update({"start": 0.0, "end": 360.0, "enabled": False})
    slider_start.set_val(0)
    slider_end.set_val(360)
    text_start.set_val("0")
    text_end.set_val("360")
    btn_toggle.label.set_text('[OFF] Filtre')
    ax_btn_tog.set_facecolor('#333333')
    redraw_angle_lines()

def on_capture(event):
    global capturing
    if capturing:
        return
    def _capture():
        global capturing, baseline_data
        capturing = True
        with baseline_lock:
            baseline_buffer.clear()
        capture_start_time[0] = time.time()
        print(f"Capture baseline ({BASELINE_WINDOW_SEC}s)...")
        time.sleep(BASELINE_WINDOW_SEC)
        result = compute_baseline()
        with baseline_lock:
            baseline_data = result
        capturing = False
        n = len(result) if result is not None else 0
        print(f"Baseline calculee : {n} pts")
        save_config()
        redraw_baseline()
    threading.Thread(target=_capture, daemon=True).start()

def on_toggle_baseline(event):
    global baseline_active
    baseline_active = not baseline_active
    if baseline_active:
        btn_baseline.label.set_text('[ON]  Aff.BL')
        ax_btn_bl.set_facecolor('#003366')
    else:
        btn_baseline.label.set_text('[OFF] Aff.BL')
        ax_btn_bl.set_facecolor('#333333')
    redraw_baseline()

def on_clear_baseline(event):
    global baseline_data, baseline_active
    with baseline_lock:
        baseline_data = None
        baseline_buffer.clear()
    baseline_active = False
    btn_baseline.label.set_text('[OFF] Aff.BL')
    ax_btn_bl.set_facecolor('#333333')
    save_config()
    redraw_baseline()

def on_diff_reset(event):
    diff_max_observed[0] = 100.0
    ax_diff.set_ylim(-100, 100)
    diff_scatter.set_clim(-100, 100)
    fig.canvas.draw_idle()

slider_start.on_changed(on_slider_start)
slider_end.on_changed(on_slider_end)
text_start.on_submit(on_text_start)
text_end.on_submit(on_text_end)
btn_toggle.on_clicked(on_toggle)
btn_reset.on_clicked(on_reset)
btn_capture.on_clicked(on_capture)
btn_baseline.on_clicked(on_toggle_baseline)
btn_clear.on_clicked(on_clear_baseline)
btn_diff_reset.on_clicked(on_diff_reset)

# ══════════════════════════════════════════════════════════
# CHARGEMENT AU DEMARRAGE
# ══════════════════════════════════════════════════════════
load_config()
slider_start.set_val(angle_filter["start"])
slider_end.set_val(angle_filter["end"])
text_start.set_val(str(int(angle_filter["start"])))
text_end.set_val(str(int(angle_filter["end"])))
if angle_filter["enabled"]:
    btn_toggle.label.set_text('[ON]  Filtre')
    ax_btn_tog.set_facecolor('#004400')
redraw_angle_lines()
redraw_baseline()

# ══════════════════════════════════════════════════════════
# ANIMATION
# ══════════════════════════════════════════════════════════
frame_count = [0]

def update(frame):
    global new_data_flag

    # ── Barre de progression ──────────────────────────────
    if capturing and capture_start_time[0] is not None:
        elapsed  = time.time() - capture_start_time[0]
        progress = min(elapsed / BASELINE_WINDOW_SEC, 1.0)
        progress_bar.set_width(progress)
        progress_bar.set_color(plt.cm.RdYlGn(progress))
        progress_label.set_text(f'{int(progress*100)}%')
        progress_label.set_color('black')
    elif baseline_data is not None:
        progress_bar.set_width(1.0)
        progress_bar.set_color('lime')
        progress_label.set_text('BL OK')
        progress_label.set_color('black')
    else:
        progress_bar.set_width(0)
        progress_label.set_text('Pret')
        progress_label.set_color('gray')

    # ── Lecture données live ──────────────────────────────
    with data_lock:
        if not new_data_flag:
            return
        angles    = latest_angles.copy()
        distances = latest_distances.copy()
        new_data_flag = False

    valid = (distances > 50) & (distances < 12000)
    a, d  = angles[valid], distances[valid]

    # ── Radar ─────────────────────────────────────────────
    if graph_enabled["radar"]:
        if len(a) > 0:
            mask_in  = apply_angle_filter(a)
            mask_out = ~mask_in
            a_in, d_in   = a[mask_in],  d[mask_in]
            a_out, d_out = a[mask_out], d[mask_out]
            if len(a_in) > 0:
                scatter_in.set_offsets(np.c_[a_in, d_in])
                scatter_in.set_color(
                    plt.cm.RdYlGn_r(np.clip(d_in / 5000, 0, 1)))
            else:
                scatter_in.set_offsets(np.empty((0, 2)))
            scatter_out.set_offsets(
                np.c_[a_out, d_out] if len(a_out) > 0
                else np.empty((0, 2)))
        else:
            scatter_in.set_offsets(np.empty((0, 2)))
            scatter_out.set_offsets(np.empty((0, 2)))

    # ── Calcul diff (toujours, pour MQTT) ─────────────────
    ang_diff, diff_vals = compute_diff(a, d)

    # ── Publication MQTT ──────────────────────────────────
    if ang_diff is not None and len(ang_diff) > 0:
        d_mean = float(diff_vals.mean())
        d_std  = float(diff_vals.std())
        n_pts  = len(diff_vals)
        publish_diff(d_mean, d_std, n_pts)
        color = 'lime' if abs(d_mean) < 100 else 'red'
        mqtt_pub_value.set_text(
            f"moy={d_mean:+.1f}mm  std={d_std:.1f}mm  n={n_pts}")
        mqtt_pub_value.set_color(color)
    else:
        mqtt_pub_value.set_text("Valeur : pas de baseline")
        mqtt_pub_value.set_color('#555555')

    # ── Graphique diff XY ─────────────────────────────────
    if graph_enabled["diff_xy"]:
        if ang_diff is not None and len(ang_diff) > 0:
            x_deg    = angles_to_display(ang_diff.copy())
            sort_idx = np.argsort(x_deg)
            x_sorted = x_deg[sort_idx]
            d_sorted = diff_vals[sort_idx]

            diff_scatter.set_offsets(np.c_[x_sorted, d_sorted])
            diff_scatter.set_array(d_sorted)

            max_r = np.abs(diff_vals).max()
            if max_r > diff_max_observed[0]:
                diff_max_observed[0] = max_r * 1.1
            diff_scatter.set_clim(-diff_max_observed[0], diff_max_observed[0])
            ax_diff.set_ylim(-diff_max_observed[0], diff_max_observed[0])

            if len(x_sorted) >= 5:
                kernel  = np.ones(5) / 5
                trend_y = np.convolve(d_sorted, kernel, mode='valid')
                trend_x = x_sorted[2: 2 + len(trend_y)]
                diff_trend.set_data(trend_x, trend_y)
            else:
                diff_trend.set_data(x_sorted, d_sorted)

            if diff_fill_pos[0] is not None:
                diff_fill_pos[0].remove()
            if diff_fill_neg[0] is not None:
                diff_fill_neg[0].remove()
            diff_fill_pos[0] = ax_diff.fill_between(
                x_sorted, d_sorted, 0,
                where=(d_sorted >= 0), color='red',  alpha=0.12, zorder=2)
            diff_fill_neg[0] = ax_diff.fill_between(
                x_sorted, d_sorted, 0,
                where=(d_sorted < 0),  color='cyan', alpha=0.12, zorder=2)

            s = angle_filter["start"] % 360
            e = angle_filter["end"]   % 360
            x_min, x_max = (s - 360, e) if s > e else (s, e)
            ax_diff.set_xlim(x_min - 2, x_max + 2)
            ticks = np.linspace(x_min, x_max, 7)
            ax_diff.set_xticks(ticks)
            ax_diff.set_xticklabels(
                [f"{t:.0f}" for t in ticks], color='#888888', fontsize=6)

            d_mean2 = diff_vals.mean()
            d_std2  = diff_vals.std()
            diff_stats.set_text(
                f"diff actuel : moy={d_mean2:+.0f}mm | "
                f"std={d_std2:.0f}mm | n={len(diff_vals)}")
            diff_stats.set_color('yellow' if abs(d_mean2) < 100 else 'red')

            # Alimentation buffer temporel
            now = time.time()
            with temporal_lock:
                temporal_buffer.append((now, float(d_mean2), float(d_std2)))
                cutoff = now - TEMPORAL_WINDOW_SEC
                while temporal_buffer and temporal_buffer[0][0] < cutoff:
                    temporal_buffer.popleft()
        else:
            diff_scatter.set_offsets(np.empty((0, 2)))
            diff_trend.set_data([], [])
            for f_ in [diff_fill_pos, diff_fill_neg]:
                if f_[0] is not None:
                    f_[0].remove(); f_[0] = None
            diff_stats.set_text("diff actuel : pas de baseline")
    else:
        # Graphique OFF mais buffer temporel alimenté quand même
        if ang_diff is not None and len(ang_diff) > 0:
            d_m = float(diff_vals.mean())
            d_s = float(diff_vals.std())
            diff_stats.set_text(
                f"diff actuel : moy={d_m:+.0f}mm [graph OFF]")
            now = time.time()
            with temporal_lock:
                temporal_buffer.append((now, d_m, d_s))
                cutoff = now - TEMPORAL_WINDOW_SEC
                while temporal_buffer and temporal_buffer[0][0] < cutoff:
                    temporal_buffer.popleft()

    # ── Graphique temporel ────────────────────────────────
    if graph_enabled["temporal"]:
        with temporal_lock:
            buf_snap = list(temporal_buffer)
        if len(buf_snap) >= 2:
            t_arr = np.array([e[0] for e in buf_snap])
            m_arr = np.array([e[1] for e in buf_snap])
            s_arr = np.array([e[2] for e in buf_snap])
            t_rel = t_arr - t_arr[-1]

            time_line.set_data(t_rel, m_arr)
            time_dot.set_data([t_rel[-1]], [m_arr[-1]])

            if time_fill_storage[0] is not None:
                time_fill_storage[0].remove()
            time_fill_storage[0] = ax_time.fill_between(
                t_rel, m_arr, 0,
                where=(m_arr >= 0), color='red',  alpha=0.15)
            ax_time.fill_between(
                t_rel, m_arr, 0,
                where=(m_arr < 0),  color='cyan', alpha=0.15)

            if band_fill_storage[0] is not None:
                band_fill_storage[0].remove()
            band_fill_storage[0] = ax_time.fill_between(
                t_rel, m_arr - s_arr, m_arr + s_arr,
                color='yellow', alpha=0.08)

            ax_time.set_xlim(-TEMPORAL_WINDOW_SEC, 0)
            y_max = max(np.abs(m_arr).max() + s_arr.max(), 50) * 1.2
            ax_time.set_ylim(-y_max, y_max)

            diff_stats2.set_text(
                f"diff 10s : moy={m_arr.mean():+.0f}mm | "
                f"std={m_arr.std():.0f}mm | "
                f"min={m_arr.min():+.0f}mm | "
                f"max={m_arr.max():+.0f}mm")

    # ── Titre radar ───────────────────────────────────────
    frame_count[0] += 1
    n_in  = int(apply_angle_filter(a).sum()) if len(a) > 0 else 0
    n_tot = int(valid.sum())
    title_radar.set_text(
        f"LD06 | Tour #{frame_count[0]} | Zone: {n_in}/{n_tot} pts"
        + (" | [BL]" if baseline_active and baseline_data is not None else "")
        + ("" if graph_enabled["radar"] else " [radar OFF]"))

ani = animation.FuncAnimation(
    fig, update, interval=50, blit=False, cache_frame_data=False)

plt.show()
