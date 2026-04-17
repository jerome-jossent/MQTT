using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MQTT_Messages_Generator
{
    internal
        class DATA_light
    {
        //public class DATA_sub1
        //{
        //    public string name { get; set; }
        //    public bool val_bool { get; set; }

        //    public DATA_sub1()
        //    {
        //        val_bool = random.Next(0, 2) == 0;
        //        name = Guid.NewGuid().ToString().Substring(0, 2);
        //    }
        //}

        static int compteur;
        static Random random = new Random();

        public bool val_bool { get; set; }
        public int val_int { get; set; }
        //public double val_float { get; set; }
        //public string val_string { get; set; }
        //public DateTime time { get; set; }

        public List<int> compteur_list { get; set; }

        //public List<DATA_sub1> datas_list { get; set; }
        //public DATA_sub1[] datas_array { get; set; }
        //public Dictionary<string, DATA_sub1> datas_dict { get; set; }

        public DATA_light()
        {
            compteur++;
            val_bool =  random.Next(0, 2) == 0;
            compteur_list = new List<int>();
            for (int i = 0; i < compteur; i++)
                compteur_list.Add(i);

            val_int = compteur;
            //val_float = random.NextDouble();
            //val_string = Guid.NewGuid().ToString();
            //time = DateTime.Now;

            //datas_list = new List<DATA_sub1>();
            //datas_dict = new Dictionary<string, DATA_sub1>();
            //for (int i = 0; i < random.Next(1, 3); i++)
            //{
            //    var d = new DATA_sub1();
            //    datas_list.Add(d);
            //    datas_dict.Add(d.name, d);
            //}
            //datas_array = datas_list.ToArray();
        }

        public string ToJSON()
        {
            return JsonSerializer.Serialize(this);
        }
    }

}
