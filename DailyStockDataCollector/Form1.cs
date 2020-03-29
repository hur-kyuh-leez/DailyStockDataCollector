/*
 kiwoom api 사용에 필요한 공부 자료 강추
 https://cafe.naver.com/zamboaprograming
 
 sqlite 연결을 위해
 설정에 필요한 것
 참고 영상: https://www.youtube.com/watch?v=ayp3tHEkRc0
 */
using Dapper;
using System.Data.SQLite;
using System.Configuration; // 기본적으로 안되어 있어서 설정 따로 해줘야 함 참고 영상 참고 바람

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DailyStockDataCollector
    /*
     이 프로그램은 장마감 후 각 종목에 투자자별 매매동향을 알기위해 만들었습니다.
     kiwoom api TR + sqlite를 이용하여 저장합니다.
     Critical Error: After around 220 + alpha TRrequests to Kiwoom server, the server does not response.
     Manual Fix: 정지된 종목 필기 후 프로그램 재시작
     */
{
    public partial class Form1 : Form
    {
        // initial variables
        int screenNum = 1000;
        string code = "";

        // use AutoResetEvent to get Code order right, without this "i" in "for loop" in "Button_Click()" takes a step faster than "API_OnReceiveTrData()"
        static AutoResetEvent objAuto = new AutoResetEvent(false); 
        public Form1()
        {
            InitializeComponent();
            LoadLastVars();

            axKHOpenAPI1.CommConnect(); //start kiwoom api
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData; //TrData가 키움 서버로 부터 왔을 시 받음

            // 일반 버튼 클릭 행동
            투자자별매매동향Button.Click += Button_Click;
            프로그램매매동향Button.Click += Button_Click;

            // Set environment variable before calling Restart

            // Detect restart:
            var wasRestarted = Environment.GetEnvironmentVariable("MYAPP_RESTART");

            if (wasRestarted == "1")
            {
                // Your app was restarted
                Environment.SetEnvironmentVariable("MYAPP_RESTART", "0");
                LoadLastVars();
                여기서부터다시시작textBox.Text = code;
            }
            else
            {
                // intial variable for textboxes
                dateTextBox.Text = DateTime.Now.ToString("yyyyMMdd");
                여기서부터다시시작textBox.Text = "";
            }
            

        }

        private void Button_Click(object sender, EventArgs e)
        {
            /*
             버튼 클릭 할 시, 명시
             */
            if (sender == 투자자별매매동향Button)
            {
                listBox.Items.Add("투자자별매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate () // 자몽님이 만들어 주신 "알바생"
                {
                    string[] codeList = getCodeList("kosdaq", 여기서부터다시시작textBox.Text);
                    for (int i=0; i< codeList.Count();i++)
                    {
                        code = codeList[i];
                        Console.WriteLine(axKHOpenAPI1.GetMasterCodeName(codeList[i]));
                        // input
                        axKHOpenAPI1.SetInputValue("일자", dateTextBox.Text);
                        axKHOpenAPI1.SetInputValue("종목코드", code);
                        axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                        axKHOpenAPI1.SetInputValue("매매구분", "0");
                        axKHOpenAPI1.SetInputValue("단위구분", "1");

                        //sRQName에 종목코드를 추가해서 Receive event에서도 code 받을수 있게 하기 이렇게 해도 되지만 너무 Thread.Sleep에 의존하는거 같아 폐기
                        //int result = axKHOpenAPI1.CommRqData("일별주가요청-" + code, "opt10059", 0, GetScreenNum());

                        int result = axKHOpenAPI1.CommRqData("일별주가요청", "opt10059", 0, GetScreenNum());
                        Thread.Sleep(3750); // 여유롭게 요청 마다 5초 줌
                        objAuto.WaitOne(); // Thread에 기달리라고 함
                    }
                });
                rqThread.Start(); //위에 정의한 Thread 알바생 시작
            }
            else if (sender == 프로그램매매동향Button)
            {
                listBox.Items.Add("프로그램매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                    string[] codeList = getCodeList("kosdaq", 여기서부터다시시작textBox.Text);
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        code = codeList[i];
                        axKHOpenAPI1.SetInputValue("시간일자구분", "2");
                        axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                        axKHOpenAPI1.SetInputValue("종목코드", code);
                        axKHOpenAPI1.SetInputValue("날짜", dateTextBox.Text);
                        int result = axKHOpenAPI1.CommRqData("프로그램일별요청", "opt90013", 0, GetScreenNum());
                        Thread.Sleep(3750);
                        objAuto.WaitOne();
                    }
                });
                rqThread.Start();
            }

        }
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            // AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent 뭐 들어 있는 지 확인
            Console.WriteLine("sErrorCode" + e.sErrorCode);
            Console.WriteLine("sMessage" + e.sMessage);
            Console.WriteLine("sPrevNext" + e.sPrevNext);
            Console.WriteLine("sRecordName" + e.sRecordName);
            Console.WriteLine("sRQName" + e.sRQName);
            Console.WriteLine("sScrNo" + e.sScrNo);
            Console.WriteLine("sTrCode" + e.sTrCode);


            if (e.sRQName.Equals("일별주가요청")) 
            {
                listBox.Items.Add("Worked On: 일별주가요청 " + code);
                for (int i = 0; i < axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int retail = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "개인투자자"));
                    int foreigner = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "외국인투자자"));
                    int insitiute = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "기관계"));
                    if (retail > 0 || foreigner > 0 || insitiute > 0)
                    {
                        // 저장할 변수들이 잘 저장되어 있는지 확인
                        //Console.WriteLine("recorded_time: " + recorded_time);
                        //Console.WriteLine("Code: " + code);
                        //Console.WriteLine("date: " + date);
                        //Console.WriteLine("개인순매수: " + retail);
                        //Console.WriteLine("외인순매수: " + foreigner);
                        //Console.WriteLine("기관순매수: " + insitiute);
                        using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                        {
                            //이미 primary key가 있으면 ignore하고 아니면 insert하셈 참고로 db에 primary keys는 code와 date로 만듬
                            cnn.Execute("insert or ignore into DailyCollectedData (recorded_time, code, date, retail, foreigner, insitiute) values (@recorded_time, @code, @date, @retail, @foreigner, @insitiute)", new { recorded_time, code, date, retail, foreigner, insitiute });
                        }
                    }
                    objAuto.Set(); // 일시정지 시킨 쓰레드 다시 시작
                }
            }
            else if (e.sRQName.Contains("프로그램일별요청"))
                // 여기는 다시 손바야함
            {
                listBox.Items.Add("Worked On: 프로그램일별요청 " + code);
                for (int i = 0; i < axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int program = dataCleansing(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "프로그램순매수수량"));

                    Console.WriteLine("code" + code);
                    Console.WriteLine("program" + program);

                    using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                    {
                        //이미 primary key가 있으면 ignore하고 아니면 insert하셈
                        cnn.Execute("update DailyCollectedData set  program = @program where (code = @code and date = @date)", new { program, code, date });
                    }
                }
                objAuto.Set();
            }
        }


        private int dataCleansing(string s)
        {
            if (s.Length > 0)
            {
                s = s.Trim();
                //if (s.Substring(0, 1).Equals("-"))
                //{
                //    s = s.Substring(1, s.Length - 1);
                //}
                return Int32.Parse(s);
            }
            else
            {
                return 0;
            }
        }
        
        private string[] getCodeList(string market="kosdaq", string interruptedCode="")
        {
            /*
             코드리스트를 받아 정리한다. 필요한 이유는 프로그램 정지 될 때, 정지된 종목부터 재실행 하기 위해 필요함
             */
            string mkt_parameter = "";
            if (market.Equals("kosdaq")) { mkt_parameter = "10"; }
            else if (market.Equals("kospi")) { mkt_parameter = "0"; }
            else { mkt_parameter = null; } // 모든 마켓
            string[] codeArray = axKHOpenAPI1.GetCodeListByMarket(mkt_parameter).Split(';'); // 마켓에 해당하는 종목 가져오기
            codeArray = codeArray.OrderBy(x => x).ToArray(); // Ascending나열 해서 프로그램 정지시킨 종목을 찾기를 위함
            codeArray = codeArray.Skip(1).ToArray(); // remove first element, which is blank
            if (interruptedCode.Equals("") == false)
            {
                int index = Array.FindIndex(codeArray, row => row == interruptedCode);
                string[] segment = codeArray.Skip(index).Take(codeArray.Count()).ToArray(); // 와 이거 하는데 정말 오래걸랬다. 근데 정확한것도 아님... [index..] 이것도 안먹음 python은 그냥 array[index:] 이러면 끝인데...
                return segment;
            }
            return codeArray;
        }
        private string GetScreenNum()
        /*
         자몽님이 만들어 주신 GetScreenNum()
         이걸 이용해 자동으로 ScreenNum 알맞게 올리기
         */
        {
            //if ((screenNum+1) % 199 == 0)
            //{
            //    SaveVars(dateTextBox.Text, code);
            //    Environment.SetEnvironmentVariable("MYAPP_RESTART", "1");
            //    Application.Restart();
            //    Environment.Exit(0);
            //}

 
            if (screenNum >= 199)
                screenNum = 1000;


            screenNum++;
            return screenNum.ToString();
        }

        private void SaveVars(string date, string code)
        {
            using (TextWriter tw = new StreamWriter("SavedVars.txt"))
            {
                tw.WriteLine(date);
                tw.WriteLine(code);
            }
        }

        private void LoadLastVars()
        {
            using (TextReader tr = new StreamReader("SavedVars.txt"))
            {
                // read lines of text
                dateTextBox.Text = tr.ReadLine();
                code = tr.ReadLine();
            }
        }
    }
}
