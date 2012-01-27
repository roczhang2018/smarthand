﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataLibrary;
using Datalibrary;
using System.IO;
using LumenWorks.Framework.IO.Csv;

namespace app
{
    public partial class Form1 : Form
    {
        private RecordStructure m_smartbufferlist;
        private IList<Record> m_recordList;
        private Record m_currentRecord ;
        private int m_indicator = 0;
        private bool m_isCatched = false;

        //filename for save
        private string m_filepath;
        private bool m_fileChanged = false; // check if user input new date


        
        public Form1()
        {
            InitializeComponent();
            // initialize the data
            string filePath = @"C:\Users\zhangroc\app\data\DB.csv";
            m_smartbufferlist = new RecordStructure(filePath);

            m_recordList = new List<Record>();
            m_currentRecord = new Record();
            m_currentRecord.Date = DateTime.Today.ToShortDateString();

            PreviousRecord.Enabled = false;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableInput(true);
            m_indicator = 0;
            this.nameText.Focus();
        }

        public void EnableInput(bool enabled)
        {
            this.nameText.Enabled    = enabled;
            this.ageText.Enabled     = enabled;
            this.sexText.Enabled     = enabled;
            this.addressText.Enabled = enabled;
            this.numberText.Enabled  = enabled;
                   
            this.calenderTimePicker.Enabled = enabled;

            this.diagnosisText.Enabled = enabled;
            this.allCostText.Enabled   = enabled;
            this.selfPayText.Enabled   = enabled;
            this.compensatePayText.Enabled = enabled;

            this.NextRecord.Enabled     = enabled;
            //// diable the previous function 
            //this.PreviousRecord.Enabled = enabled; 
            

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }


        private void SetCurrentRecord(PeopleInfo people)
        {
            this.numberText.Text = people.No;
            this.ageText.Text = people.Age.ToString();
            this.sexText.Text = people.Sex;
            this.addressText.Text = people.Address;

            m_currentRecord.People = people;
        }

        private void nameText_KeyUp(object sender, KeyEventArgs e)
       {
            if(e.KeyValue == 13 && ! String.IsNullOrEmpty(nameText.Text.Trim()))
            {
                m_currentRecord.Name = nameText.Text.Trim();
                List<PeopleInfo> peopleList = new List<PeopleInfo>();
                m_isCatched = m_smartbufferlist.Query(m_currentRecord.Name, ref peopleList);
                if (m_isCatched)
                {

                    SetCurrentRecord(peopleList.First());
                    
                    this.diagnosisText.Focus();
                }
                else
                {
                    this.numberText.Focus();
                }
                
               ;
                
            }

            
        }

        private void diagnosisText_KeyUp(object sender, KeyEventArgs e)
        {
            if ( e.KeyValue == 13 &&!String.IsNullOrEmpty(diagnosisText.Text.Trim()))
            {
                m_currentRecord.Diagnose = this.diagnosisText.Text.Trim();
                allCostText.Focus();
            }
        }

        private void allCostText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !String.IsNullOrEmpty(allCostText.Text.Trim()))
            {
                
                float allcost;
                if(!float.TryParse(this.allCostText.Text.Trim(),out allcost))
                {
                    ShowMesage(@"总费用应该是数字");
                    allCostText.Text = "";
                    allCostText.Focus();
                    return;
                }
                if (!ValidateMorethanZero(allcost))
                {
                    ShowMesage(@"总费用不能小于0！");
                    allCostText.Text = "";
                    allCostText.Focus();
                    return;
                }

                CostFormular cf = new CostFormular(allcost);

                m_currentRecord.AllCost = cf.AllCost;
                m_currentRecord.Compensation = cf.Compentation;


                this.allCostText.Text = m_currentRecord.AllCost.ToString("F2");
                this.selfPayText.Text = m_currentRecord.SelfPay.ToString("F2");
                this.compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");

                compensatePayText.Focus();
            }
        }

        private void ShowMesage(string val)
        {
            if (!String.IsNullOrEmpty(val))
            {
                MessageBox.Show(val);
            }
               
        }

        private void compensatePayText_KeyUp(object sender, KeyEventArgs e)
        {


             if (e.KeyValue == 13 && !String.IsNullOrEmpty(compensatePayText.Text.Trim()))
             {
                 string newValue = compensatePayText.Text.Trim();
                 string oldValue = m_currentRecord.Compensation.ToString();
    
                 if (  String.Compare(newValue, oldValue) != 0)
                 {
                     float newVal;
                     if(!float.TryParse(newValue,out newVal))
                     {
                         ShowMesage(@"总费用应该是数字");
                         compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");
                         compensatePayText.Focus();
                         return;
                     }

                     if (! ValidateMorethanZero(newVal))
                     {
                         ShowMesage(@"补偿费应该大于0");
                         compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");
                         compensatePayText.Focus();
                         return;
                     }

                     if (newVal - m_currentRecord.AllCost > 0)
                     {
                         ShowMesage("补偿费("+newVal.ToString("F2")+")不能大于总费用("+m_currentRecord.AllCost.ToString("F2")+")!");
                         compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");
                         compensatePayText.Focus();
                         return;
                     }

                     m_currentRecord.Compensation = newVal;
                     compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");
                     selfPayText.Text = m_currentRecord.SelfPay.ToString("F2");
                 }
                 
                  NextRecord.Focus();

             }
        }

        private bool ValidateMorethanZero(float val)
        {
            return val > 0.0f;
        }

        private void selfPayText_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyValue == 13 && !String.IsNullOrEmpty(selfPayText.Text.Trim()))
            {
                float selfPay;
                string newValue = selfPayText.Text.Trim();
                string oldValue = m_currentRecord.SelfPay.ToString();
                if (String.Compare(newValue, oldValue) != 0)
                {
                    if (!float.TryParse(selfPayText.Text.Trim(), out selfPay))
                    {
                        ShowMesage(@"总费用应该是数字");
                        selfPayText.Text = m_currentRecord.SelfPay.ToString("F2");
                        selfPayText.Focus();
                        return;
                    }

                    if (!ValidateMorethanZero(selfPay))
                    {
                        ShowMesage(@"自付费用不能小于0！");
                        selfPayText.Text = m_currentRecord.SelfPay.ToString("F2");
                        selfPayText.Focus();
                        return;
                    }

                    if (selfPay > m_currentRecord.AllCost)
                    {
                        ShowMesage(@"自付费" + selfPay.ToString("F2") + "用不能大于总费用" + m_currentRecord.AllCost.ToString("F2") +
                                   ")!");
                        selfPayText.Text = m_currentRecord.SelfPay.ToString("F2");
                        selfPayText.Focus();
                        return;
                    }


                    m_currentRecord.Compensation = m_currentRecord.AllCost - selfPay;

                    compensatePayText.Text = m_currentRecord.Compensation.ToString("F2");
                    selfPayText.Text = selfPay.ToString("F2");
                }
                
                NextRecord.Focus();
               
            }
        }
        private void NextRecord_Click(object sender, EventArgs e)
        {
           
           
           // create the new record
           if ( m_recordList.Count == 0 || m_indicator == m_recordList.Count -1  )
           {
               if (!String.IsNullOrEmpty(nameText.Text.Trim()) &&
                   !String.IsNullOrEmpty(numberText.Text.Trim()) &&
                   !String.IsNullOrEmpty(addressText.Text.Trim()) &&
                   !String.IsNullOrEmpty(ageText.Text.Trim()) &&
                   !String.IsNullOrEmpty(sexText.Text.Trim()) &&
                   !String.IsNullOrEmpty(diagnosisText.Text.Trim()) &&
                   !String.IsNullOrEmpty(allCostText.Text.Trim()) &&
                   !string.IsNullOrEmpty(selfPayText.Text.Trim())&&
                   !string.IsNullOrEmpty(compensatePayText.Text.Trim()))
               {
                  
                   //when open exsite file ,the current text box display the last record from m_recordlist
                   //Now add the record it will create duplicate code.
                   if(String.IsNullOrEmpty(m_currentRecord.Name))
                   {
                       ClearUI();

                       //update the previous button
                       m_indicator = m_recordList.Count - 1;
                       string tlabel = Indicator2Position(m_indicator) + "/" + m_recordList.Count.ToString();
                       UpdateIndicator(m_indicator, -1, -1, tlabel);

                       return;
                   }

                   SaveCurrentRecord();
                   m_fileChanged = true;
                   
                   m_indicator = m_recordList.Count - 1;

                   int previous = m_indicator;
                   int current = -1;
                   int next = -1;
                   string label = Indicator2Position(m_indicator) + "/" + m_recordList.Count.ToString();
                   UpdateIndicator(previous, current, next, label);

                   UpdateSmartBuffer();
                   ClearUI();
                   nameText.Focus();
               }
               else
               {
                   ShowMesage(@"请检查，纪录需要完整输入。");
               }
           }
           else
           {
               m_indicator++;

               int current = m_indicator;
               int previous = m_indicator - 1;
               int next = m_indicator + 1;
               if ( next > m_recordList.Count - 1)
                   next = m_recordList.Count - 1;

               string lableStatus = Indicator2Position(m_indicator).ToString() + "/" + m_recordList.Count.ToString();
               UpdateIndicator( previous, current,next, lableStatus);
           }
          
            

        }

        private void UpdateSmartBuffer()
        {
           if( m_isCatched )
               return;
            m_smartbufferlist.AddRecord(m_currentRecord.People);
        }


        private void UpdateIndicator(int previous, int current, int next, string label)
        {
            UpdateInputUI(current);
            if( previous >= 0)
            {
                PreviousRecord.Text = @"上一个: " + m_recordList[previous].BasicInfo();
                //PreviousRecord.Text = m_recordList[previous].BasicInfo();
                
            }

            if( next > 0 )
            {
                NextRecord.Text = @"下一个: " + m_recordList[next].BasicInfo(); 
            }

            if (! ReferenceEquals( label, null))
            {
                indicatorLable.Text = label;            
            }
        }

        private void UpdateInputUI(int position)
        {
            if (position >= 0)
            {
                Record current = m_recordList[position];

                nameText.Text = current.Name;
                numberText.Text = current.No;
                ageText.Text = current.Age.ToString();
                sexText.Text = current.Sex;
                addressText.Text = current.Address;

                calenderTimePicker.Text = current.Date;

                diagnosisText.Text = current.Diagnose;
                allCostText.Text = current.AllCost.ToString("F2");
                compensatePayText.Text = current.Compensation.ToString("F2");
                selfPayText.Text = current.SelfPay.ToString("F2");
            }
        }
       
        private void SaveCurrentRecord()
        {
            Record r =new Record(m_currentRecord);
            m_recordList.Add(r);
        }

        private void ClearUI()
        {
            this.nameText.Clear();
            this.ageText.Clear();
            this.sexText.Clear();
            this.addressText.Clear();
            this.numberText.Clear();

            this.diagnosisText.Clear();
            this.allCostText.Clear();
            this.selfPayText.Clear();
            this.compensatePayText.Clear();

            m_currentRecord.Clear();
        }

        private void calenderTimePicker_ValueChanged(object sender, EventArgs e)
        {
            m_currentRecord.Date = this.calenderTimePicker.Text;
        }

        private void numberText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !String.IsNullOrEmpty(numberText.Text.Trim()))
            {
                m_currentRecord.No = numberText.Text.Trim();

                addressText.Focus();
            }
        }

        private void addressText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !String.IsNullOrEmpty(addressText.Text.Trim()))
            {
                m_currentRecord.Address = addressText.Text.Trim();

                ageText.Focus();
            }

        }

        private void ageText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !String.IsNullOrEmpty(ageText.Text.Trim()))
            {
                float age;
                if( !float.TryParse(ageText.Text.Trim(),out age))
                {
                    ShowMesage(@"年龄必须是数值");
                    ageText.Text = "";
                    return;
                }
                m_currentRecord.Age = age;

                sexText.Focus();
            }
        }

        private void sexText_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13 && !String.IsNullOrEmpty(sexText.Text.Trim()))
            {
                string temp= sexText.Text.Trim();
                if( temp == @"男" || temp == "女")
                {
                    m_currentRecord.Sex = temp;
                    diagnosisText.Focus();
                }
                else
                {
                    ShowMesage("性别只能是男或女");
                    sexText.Text = "";
                    sexText.Focus();
                }
            }
        }

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // check the current whether it has already open new file
            // create new file and input data but no save so far
            if ((String.IsNullOrEmpty(m_filepath) && m_recordList.Count != 0) ||
                 m_fileChanged == true)
            {

                if (MessageBox.Show("还没有保存当前输入，创建新文件会丢失已经输入信息。\r\n" +
                                    " 你确实丢掉已输入信息而创建新文件吗?", "保存提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
            
            
            //step1 open file and save the filepath
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "csv(*.csv)|*.csv|all(*.*)|*.*";
            
            if ( !String.IsNullOrEmpty(m_filepath))
                ofd.InitialDirectory = m_filepath;
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                m_filepath = ofd.FileName;
            }

            try
            {
                //step2 initialize the recordlist
                 m_recordList.Clear();
                 ReaderCSV(m_filepath);
                //m_currentRecord = m_recordList.Last();
            }
            catch(Exception val)
            {
                ShowMesage(val.Message + "\r\n"+ 
                    "问题：该文件被其他应用程序已打开" +"\r\n"+
                    "建议：关掉其他应用程序，重新打开");
                return;
            }
            

            //step3 update UI
            if( m_recordList.Count > 0)
            {
                m_indicator = m_recordList.Count - 1;
                UpdateInputUI(m_indicator);

                //step4 update the indicator
                int previous = m_indicator - 1;
                if (previous < 0)
                    previous = 0;

                //step5 update the indicator
                int next = -1; // don't initial the next button

                string label = Indicator2Position(m_indicator).ToString() + "/" + m_recordList.Count.ToString();
                UpdateIndicator(previous, m_indicator, next, label);


                //enable UI
                EnableInput(true);
            }



        }

        private void ReaderCSV(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            if ( !Directory.Exists(path))
                return;

            // open the file "data.csv" which is a CSV file with headers
            using (CsvReader csv =
                   new CsvReader(new StreamReader(filePath, Encoding.GetEncoding("gb2312")), true))
            {
                int fieldCount = csv.FieldCount;

                int igore = 3; // igore the first 4 line ( 0 , 1, 2, 3)
                while (csv.ReadNextRecord())
                {
                   if (igore > 0) // igore header in the first 4 lines
                    {
                        igore--;
                        continue;
                    }
                          
                    // create one record
                   // //乡村名称	合疗证号	患者姓名	年龄	性别	就诊时间	诊 断	 总医药费	药品费	检查费	治疗费	材料费	自付	补偿
          
                    string address = csv[0].Trim().Replace("?","");
                    string no = csv[1].Trim().Replace("?","");
                    string name = csv[2].Trim().Replace("?","");

                    if ( string.IsNullOrEmpty(address) || string.IsNullOrEmpty(no) || string.IsNullOrEmpty(name))
                        continue;

                    float age = float.Parse(csv[3].Trim().Replace("?",""));
                    string sex = csv[4].Trim().Replace("?","");
                    PeopleInfo p = new PeopleInfo(name,no,age,address,sex);

                    string date = csv[5].Trim().Replace("?","");
                    string diagose = csv[6].Trim().Replace("?","");
                    float allcost = float.Parse(csv[7].Trim().Replace("?",""));
                    //float selfPay = csv[12];
                    float compenation = float.Parse(csv[13].Trim().Replace("?",""));

                    Record r = new Record(p,diagose,allcost,compenation,date);
              
                    m_recordList.Add(r);
                }
                
            }
        }

        private void PreviousRecord_Click(object sender, EventArgs e)
        {
            m_indicator--;

            int current = 0;
            int next = 0;
            int previous = 0;
            if( m_indicator == -1 )
            {
                m_indicator = 0;
                previous = current = m_indicator;
                next = current + 1;
                if (next > m_recordList.Count - 1)
                    next = m_recordList.Count - 1; 
            }
            else
            {
                current = m_indicator + 1;
                previous = m_indicator;
                if (previous == -1)
                    previous = 0;

                next = m_indicator + 2;
                if (next >= m_recordList.Count - 1)
                    next = -1;

                
            }

            string label = Indicator2Position(m_indicator).ToString() + "/" + m_recordList.Count;
            UpdateIndicator(previous, current, next, label);
        }

        private int Indicator2Position(int indicator)
        {
            return indicator + 1;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

            if(String.IsNullOrEmpty(m_filepath))
            {
                GetFilePath();
            }


            if (!String.IsNullOrEmpty(m_filepath))
            {
                try
                {
                    SaveFile(m_filepath);
                }
                catch (Exception val)
                {
                    ShowMesage(val.Message + "\r\n" +
                     "问题：该文件被其他应用程序已打开" + "\r\n" +
                     "建议：关掉其他应用程序，重新打开");
                    return;
                }
                m_fileChanged = false;
            }
        }

        private void GetFilePath()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "csv(*.csv)|*.csv";
            sfd.AddExtension = true;
            sfd.ValidateNames = true;
            sfd.CheckPathExists = true; 

            sfd.ShowDialog();
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                m_filepath = sfd.FileName;
            }
        }

        private void SaveFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter streamWriter = new StreamWriter(fs, Encoding.GetEncoding("gb2312"));

            //step1:  write header

            //,,普集街,卫生院（村卫生室或诊所）新型农村合作医疗门诊统筹补偿登记表,,,,,,,,,,,,,,,,,,,,,,,,
            //2012,,,年                                  ,1,月,卫生院    （村卫生室或诊所 ）              ,,,代号,1119,,,,,,,,,,,,,,,,,
            //,填报单位： （盖章）,,年                                  ,,,,,,,,,,,,,,,,,,,,,,,,
            //乡村名称	合疗证号	患者姓名	年龄	性别	就诊时间	诊 断	 总医药费	药品费	检查费	治疗费	材料费	自付	补偿
            string line1 = @",,普集街,卫生院（村卫生室或诊所）新型农村合作医疗门诊统筹补偿登记表,,,,,,,,,,,";
            string line2 = @"2012,,,年                                  ,1,月,卫生院    （村卫生室或诊所 ）             ,,,,代号,1119,,,";
            string line3 = @",填报单位： （盖章）,,年                                  ,,,,,,,,,,";

                            //普中, 0110130030345,赵麦芳,80,女, 2012/1/14,高冠心,17.70 ,,,,,5.70 ,12.00 ,,,,,,,,,,,,,,
            string line4 = @"乡村名称,合疗证号,患者姓名,年龄,性别,就诊时间,诊断,总医药费,药品费,检查费,治疗费,材料费,自付,补偿,,";

            streamWriter.WriteLine(line1);
            streamWriter.WriteLine(line2);
            streamWriter.WriteLine(line3);
            streamWriter.WriteLine(line4);

            //write data
            foreach (Record r in m_recordList)
            {
                streamWriter.WriteLine(r.ToString());
            }
            streamWriter.Flush();
            fs.Close();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // create new file and input data but no save so far
            if(( String.IsNullOrEmpty(m_filepath) && m_recordList.Count != 0 ) ||
                 m_fileChanged == true)
            {
              
                if (MessageBox.Show("还没有保存文件，需要退出吗?","保存提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    return;
                }
            }
            

            //releaes the system resource

            Application.Exit();

            // system will automate to save the smart buffer to file.
        }

        private void SaveAsStripMenuItem2_Click(object sender, EventArgs e)
        {
            GetFilePath();
            if(!String.IsNullOrEmpty(m_filepath))
            {
                try
                {
                    SaveFile(m_filepath);
                }
                catch (Exception val)
                {
                    ShowMesage(val.Message + "\r\n" +
                     "问题：该文件被其他应用程序已打开" + "\r\n" +
                     "建议：关掉其他应用程序，重新打开");
                    return;
                }
                
                m_fileChanged = false;
            }
        }

        private void statisticsPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int peopleCount = m_recordList.Count;
            float allCost = 0.0f;
            float compentationCost = 0.0f;
            float selfCost =0.0f;
            foreach (Record r in m_recordList)
            {
                allCost += r.AllCost;
                compentationCost += r.Compensation;
            }
            selfCost = allCost - compentationCost;
            MessageBox.Show("人数     ：" + peopleCount.ToString("F2") + "\r\n" +
                            "总费用 ：" + allCost.ToString("F2") + "\r\n" +
                            "补偿     ：" + compentationCost.ToString("F2") + "\r\n" +
                            "自付费 ：" + selfCost.ToString("F2"),
                            "统计信息", MessageBoxButtons.OK);

        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowMesage("升级后实现");
        }

        private void InfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("作者：张亚光 \r\n推广者： 张美红 张光", "作者介绍", MessageBoxButtons.OK);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // create new file and input data but no save so far
            if ((String.IsNullOrEmpty(m_filepath) && m_recordList.Count != 0) ||
                 m_fileChanged == true)
            {

                if (MessageBox.Show("还没有保存文件，需要退出吗?", "保存提示", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    e.Cancel = true;
                    return;

                }
            }
        }

        private void introductionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show( "农村医疗合作信息录入助手简介：\r\n"+ @"该软件简称:'小助手'，"+
                "帮你快速实现医疗信息录入并声称报表，其特点准确，高效。" + "让你轻松跨越数字鸿沟！",
                "软件介绍", MessageBoxButtons.OK);

        }





    }
}
