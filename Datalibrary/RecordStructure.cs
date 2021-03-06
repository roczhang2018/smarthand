﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLibrary;
using LumenWorks.Framework.IO.Csv;
using System.IO;

namespace Datalibrary
{
    public class RecordStructure
    {
        private string m_filePath;
        private Dictionary<string, List<PeopleInfo>> m_people;

        public RecordStructure(string filePath)
        {
            m_filePath = filePath;
            m_people = new Dictionary<string, List<PeopleInfo>>();
            InitDataStructure();
        }

         ~RecordStructure()
        {
             SaveFile(m_filePath);
        }
        public Boolean Query(string name, ref List<PeopleInfo> recordlist )
        {
            if ( m_people.ContainsKey( name))
            {
                recordlist = m_people[name];
                return true;
            }
            return false;

        }

        private void InitDataStructure()
        {
            if (String.IsNullOrEmpty(m_filePath) || !File.Exists(m_filePath))
            {
                // default path ./data/DB.cs
                m_filePath = ".\\data\\DB.csv";
            }
            if(!File.Exists(m_filePath))
            {
                // there is no smart buffer file
                return;
            }


            using (CsvReader csv =
                   new CsvReader(new StreamReader(m_filePath, Encoding.GetEncoding("gb2312")), true))
            {
                int fieldCount = csv.FieldCount;
                int lineNumber = 0;
                while (csv.ReadNextRecord())
                {

                    for (int i = 0; i < fieldCount; i++)
                    {

                        if (String.IsNullOrEmpty(csv[i].Trim())  )
                        {
                           continue;
                        }

                       // Console.WriteLine("{0}: {1} {2} {3} {4} {5}", lineNumber, 
                       //  csv[0],csv[1],csv[2],csv[3],csv[4]);
                    }


                    if (! String.IsNullOrEmpty(csv[0].Trim()))
                    {
                        string address = csv[0].Trim();
                        string no = csv[1].Trim();
                        string tname = csv[2].Trim();
                        string name = tname.Replace(" ", "");
                        float age = float.Parse(csv[3].Trim());
                        string sex = csv[4].Trim();
                        PeopleInfo people = new PeopleInfo(name, no, age, address, sex);

                        AddRecord(people);

                    }

                    lineNumber++;

                }
            }
        }

        public  void AddRecord(PeopleInfo record)
        {
 
            if ( m_people.ContainsKey( record.Name))
            {
                List<PeopleInfo> recordlist = m_people[record.Name];
                
                foreach(PeopleInfo people in recordlist)
                {
                    if (people == record)
                    {
                        return;
                    }
                }

                recordlist.Add(record);

            }
            else
            {
                List<PeopleInfo> recordlist = new List<PeopleInfo>();
                PeopleInfo p = new PeopleInfo(record);
                recordlist.Add(p);
                m_people.Add(record.Name, recordlist);
            }
        }


        internal void vist()
        {
            int i = 0;
            foreach ( KeyValuePair<string, List<PeopleInfo>> people in m_people)
            {
                Console.Write(i + ":");
                i++;
                Console.WriteLine(people.Value.First().ToString());
            }
        }

        public void SaveFile(string path)
        {
            if(!Directory.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            FileStream fs = new FileStream(path, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));
            string header = @"乡村名称,合疗证号,患者姓名,年龄,性别,";
            streamWriter.WriteLine(header);

            foreach (KeyValuePair<string, List<PeopleInfo>> pair in m_people)
            {
                PeopleInfo p = pair.Value.First();
                string record = p.Address + "," + p.No + "," + p.Name + "," +  p.Age + "," + p.Sex + ",";
                streamWriter.WriteLine(record);
            }
            streamWriter.Flush();
            fs.Close();
        }


    }
}
