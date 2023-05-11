#from importlib.metadata import metadata
import paho.mqtt.client as mqtt
from PIL import Image, ExifTags
import io

ip = "127.0.0.1"
port = 1883
topic = "video/frame/data"

# Récupérartion des métadatas stockés dans le tag "ImageDescription"
def get_metadatas(img, tagname = "ImageDescription"):
    img_exif = img.getexif()
    if img_exif is not None:        
        for key, val in img_exif.items():
            if ExifTags.TAGS[key] == tagname:
                return val
    return ""

# The callback for when the client receives a response from the server.
def on_connect(client, userdata, flags, rc):
    print("Connected with result code " + str(rc))

    # Subscribing
    client.subscribe(topic)

# The callback for when a PUBLISH message is received from the server.
def on_message(client, userdata, msg):
    img = Image.open(io.BytesIO(msg.payload))
    metadatas = get_metadatas(img)
    print(metadatas)
        
        
client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect(ip, port, 60)

# Blocking call that processes network traffic, dispatches callbacks and handles reconnecting.
# Other loop*() functions are available that give a threaded interface and a manual interface.
client.loop_forever()