using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;
using Newtonsoft;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace CSVaperturasSAPII
{
    /// <summary>
    /// Clase program - Generar archivo csv a partir de un objeto json obtenido a través de un servicio web
    /// Modificar URL del servicio web y path donde se desea cargar el archivo
    /// </summary>
    public static class Program
    {        
        /// <summary>
        /// Método principal
        /// </summary>
        /// <param name="args">args main</param>
        static void Main(string[] args)
        {                        
            DateTime localDate = DateTime.Now;            
            String[] fecha = localDate.Date.ToString().Replace("/","-").Split(' ');
            string json = LlamarWebService("http://URL_del_Servicio_web_que_te_devuelve_objeto_json", /*"2015-11-30");*/fecha[0]);
            json = json.Replace("\\n", " ");
            json = json.Replace("\\u000a", " ");
                                    
            XmlNode xml = (XmlDocument)JsonConvert.DeserializeXmlNode("{records:{record:" + json + "}}");            

            XmlDocument xmldoc = new XmlDocument();
            //Create XmlDoc Object
            xmldoc.LoadXml(xml.InnerXml);
            //Create XML Steam 
            var xmlReader = new XmlNodeReader(xmldoc);
            DataSet dataSet = new DataSet();            
            //Load Dataset with Xml
            dataSet.ReadXml(xmlReader);                        
            if (dataSet.Tables.Count > 0)
                ToCSV(dataSet.Tables[0], ",");
            else
                ToCSV(null, null);

        }
        /// <summary>
        /// Convierte DataTable a una cadena preparada para ser escrita en un archivo CSV
        /// </summary>
        /// <param name="table">Tabla con información</param>
        /// <param name="delimator">Caracter delimitador de datos</param>
        /// <returns>Cadena lista para ser escrita</returns>
        public static string ToCSV(this DataTable table, string delimator)
        {                        
            var result = new StringBuilder();
            if (table == null)
                WriteCSV("Sin datos");
            else
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    result.Append(table.Columns[i].ColumnName);
                    result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
                }
                foreach (DataRow row in table.Rows)
                {
                    for (int i = 0; i < table.Columns.Count; i++)
                    {
                        result.Append(row[i].ToString());
                        result.Append(i == table.Columns.Count - 1 ? "\n" : delimator);
                    }
                }
                WriteCSV(result.ToString().TrimEnd(new char[] { '\r', '\n' }));
            }
            return result.ToString().TrimEnd(new char[] { '\r', '\n' });            
        }
        /// <summary>
        /// Escribe datos en archivo CSV
        /// </summary>
        /// <param name="result">Datos a escribir</param>
        public static void WriteCSV(String result)
        {
            string csvpath = @"c:\temp\Aperturas.csv";

            if (File.Exists(csvpath))
            {
                File.Delete(csvpath);
            }

            using (StreamWriter sw = File.AppendText(csvpath))
            {                
                sw.WriteLine("Aperturas");
                sw.Write(result);                
            }
        }
        /// <summary>
        /// Llama web service 
        /// </summary>
        /// <param name="URL">Url del servicio web</param>
        /// <param name="ParametrosURL">Parametros de la peticion</param>
        /// <param name="Encriptar">Bool</param>
        /// <returns>respuesta del servicio -> datos</returns>
        public static string LlamarWebService(string URL, string ParametrosURL = "", Boolean Encriptar = true)
        {
            string url;            
            try
            {
                if (ParametrosURL != "")
                {                                     
                    url = URL + "," + ParametrosURL;
                }
                else
                    url = URL;
                var request = WebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";
                string text;
                var response = (HttpWebResponse)request.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    text = sr.ReadToEnd();                    
                }
                return text;                
            }
            catch (Exception ex)
            {                
                return ex.Message;
            }
        }        
    }
}
