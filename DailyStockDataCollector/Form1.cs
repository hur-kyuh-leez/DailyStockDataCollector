/*
 kiwoom api 사용에 필요한 공부 자료 강추
 https://cafe.naver.com/zamboaprograming
 
 sqlite 연결을 위해
 설정에 필요한 것
 참고 영상: https://www.youtube.com/watch?v=ayp3tHEkRc0
 */
using Dapper;
using System.Data.SQLite;
using System.Configuration; // 기본적으로 안되어 있어서 설정 따로 해줘야 함 참고 영상 참고 바람 References->오른쪽클릭-> Add Reference에서 System.Configuration 추가

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
using System.Diagnostics;

// dual api
using AxKHOpenAPILib;


namespace DailyStockDataCollector
    /*
     이 프로그램은 장마감 후 각 종목에 투자자별 매매동향을 알기위해 만들었습니다.
     kiwoom api TR + sqlite를 이용하여 저장합니다.
     Critical Error: After around 220 + alpha TRrequests to Kiwoom server, the server does not response.
     Manual Fix: 정지된 종목 필기 후 프로그램 재시작
     fixed: GetScreenNum가 문제였음
     */
{
    public partial class Form1 : Form
    {
        // initial variables
        int screenNum = 1000;
        string code = "";
        int sleepTime = 3750;
        int timeOut = 20 * 1000;
        Stopwatch sw;
        string[] codeList;
        int codeListCount;


        // dual api

        // use AutoResetEvent to get Code order right, without this "i" in "for loop" in "Button_Click()" takes a step faster than "API_OnReceiveTrData()"
        static AutoResetEvent objAuto = new AutoResetEvent(false); 
        public Form1()
        {
            InitializeComponent();
            axKHOpenAPI1.OnEventConnect += API_OnEventConnect;
            axKHOpenAPI2.CommConnect(); //start kiwoom api2
            axKHOpenAPI1.CommConnect(); //start kiwoom api

            LoadLastVars();
            
            axKHOpenAPI1.OnReceiveTrData += API_OnReceiveTrData; //TrData가 키움 서버로 부터 왔을 시 받음
            axKHOpenAPI2.OnReceiveTrData += API_OnReceiveTrData; //TrData가 키움 서버로 부터 왔을 시 받음


            // 일반 버튼 클릭 행동
            투자자별매매동향Button.Click += Button_Click;
            프로그램매매동향Button.Click += Button_Click;
            주식기본정보Button.Click += Button_Click;
            일별가격정보Button.Click += Button_Click;
            종목명저장Button.Click += Button_Click;

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
        private void API_OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
            // 로그인 성공하면...
        {
            if (e.nErrCode == 0) progressLabel.Text = "로그인 성공!";
        }

        private void Button_Click(object sender, EventArgs e)
        {
            /*
             버튼 클릭 할 시, 명시
             */
            if (sender == 투자자별매매동향Button)
            {
                CheckDataExist("종목별투자자기관별요청");
                listBox.Items.Add("투자자별매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate () // 자몽님이 만들어 주신 "알바생"
                {
                    for (int i=0; i < codeListCount; i++)
                    {
                        if (i % 2 == 0)
                        {
                            Console.WriteLine(axKHOpenAPI1.GetMasterCodeName(codeList[i]));

                            // input
                            axKHOpenAPI1.SetInputValue("일자", dateTextBox.Text);
                            axKHOpenAPI1.SetInputValue("종목코드", codeList[i]);
                            axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                            axKHOpenAPI1.SetInputValue("매매구분", "0");
                            axKHOpenAPI1.SetInputValue("단위구분", "1");

                            //sRQName에 종목코드를 추가해서 Receive event에서도 code 받을수 있게 하기 이렇게 해도 되지만 너무 Thread.Sleep에 의존하는거 같아 폐기 //dual api를 위해서 이걸로 다시 사용하기로
                            //int result = axKHOpenAPI1.CommRqData("일별주가요청-" + code, "opt10059", 0, GetScreenNum());

                            int result = axKHOpenAPI1.CommRqData("종목별투자자기관별요청_" + codeList[i], "opt10059", 0, GetScreenNum());
                                                         //objAuto.WaitOne(timeOut); // Thread에 signal 올 때까지 기달리고 만약 20초이내 없으면 패스
                        }
                        else
                        {
                            Console.WriteLine(axKHOpenAPI1.GetMasterCodeName(codeList[i]));
                            // input
                            axKHOpenAPI2.SetInputValue("일자", dateTextBox.Text);
                            axKHOpenAPI2.SetInputValue("종목코드", codeList[i]);
                            axKHOpenAPI2.SetInputValue("금액수량구분", "2");
                            axKHOpenAPI2.SetInputValue("매매구분", "0");
                            axKHOpenAPI2.SetInputValue("단위구분", "1");

                            //sRQName에 종목코드를 추가해서 Receive event에서도 code 받을수 있게 하기 이렇게 해도 되지만 너무 Thread.Sleep에 의존하는거 같아 폐기
                            //int result = axKHOpenAPI1.CommRqData("일별주가요청-" + code, "opt10059", 0, GetScreenNum());

                            int result = axKHOpenAPI2.CommRqData("종목별투자자기관별요청_" + codeList[i], "opt10059", 0, GetScreenNum());
                            //objAuto.WaitOne(timeOut); // Thread에 signal 올 때까지 기달리고 만약 20초이내 없으면 패스
                            Thread.Sleep(sleepTime); // 여유롭게 요청 마다 5초 줌
                        }
                    }
                });
                rqThread.Start(); //위에 정의한 Thread 알바생 시작
            }
            else if (sender == 프로그램매매동향Button)
            {
                CheckDataExist("프로그램일별요청");
                listBox.Items.Add("프로그램매매동향 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                   
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        if (i % 2 == 0)
                        {
                            axKHOpenAPI1.SetInputValue("시간일자구분", "2");
                            axKHOpenAPI1.SetInputValue("금액수량구분", "2");
                            axKHOpenAPI1.SetInputValue("종목코드", codeList[i]);
                            axKHOpenAPI1.SetInputValue("날짜", dateTextBox.Text);
                            int result = axKHOpenAPI1.CommRqData("프로그램일별요청_" + codeList[i], "opt90013", 0, GetScreenNum());
                        }
                        else
                        {
                            axKHOpenAPI2.SetInputValue("시간일자구분", "2");
                            axKHOpenAPI2.SetInputValue("금액수량구분", "2");
                            axKHOpenAPI2.SetInputValue("종목코드", codeList[i]);
                            axKHOpenAPI2.SetInputValue("날짜", dateTextBox.Text);
                            int result = axKHOpenAPI2.CommRqData("프로그램일별요청_" + codeList[i], "opt90013", 0, GetScreenNum());
                            Thread.Sleep(sleepTime);
                        }
                        //objAuto.WaitOne(timeOut);
                    }
                });
                rqThread.Start();
            }
            else if (sender == 주식기본정보Button)
            {
                listBox.Items.Add("주식기본정보 저장 클릭");
                codeList = getCodeList(여기서부터다시시작textBox.Text);
                Thread rqThread = new Thread(delegate ()
                {
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        if (i % 2 == 0)
                        {
                            axKHOpenAPI1.SetInputValue("종목코드", codeList[i]);
                            int result = axKHOpenAPI1.CommRqData("주식기본정보요청_" + codeList[i], "opt10001", 0, GetScreenNum());
                        }
                        else
                        {
                            axKHOpenAPI2.SetInputValue("종목코드", codeList[i]);
                            int result = axKHOpenAPI2.CommRqData("주식기본정보요청_" + codeList[i], "opt10001", 0, GetScreenNum());
                            Thread.Sleep(sleepTime);
                        }
                        //objAuto.WaitOne(timeOut);
                    }
                });
                rqThread.Start();
            }
            else if (sender == 일별가격정보Button)
            {
                CheckDataExist("주식일주월시분요청");
                listBox.Items.Add("일별가격정보 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        if (i % 2 == 0)
                        {
                            axKHOpenAPI1.SetInputValue("종목코드", codeList[i]); //input에 날짜가 없으니... 장중에는 하지 말고 18:00 이후에 저장 바람
                            int result = axKHOpenAPI1.CommRqData("주식일주월시분요청_" + codeList[i], "opt10005", 0, GetScreenNum());
                        }
                        else
                        {
                            axKHOpenAPI2.SetInputValue("종목코드", codeList[i]); //input에 날짜가 없으니... 장중에는 하지 말고 18:00 이후에 저장 바람
                            int result = axKHOpenAPI2.CommRqData("주식일주월시분요청_" + codeList[i], "opt10005", 0, GetScreenNum());
                            Thread.Sleep(sleepTime);
                        }
                        //objAuto.WaitOne(timeOut);
                    }
                });
                rqThread.Start();
            }
            else if (sender == 종목명저장Button)
                // 이거 왜만들었지? 파이썬으로 분석할때 쓰려고 했나? 그렇다
            {
                codeList = getCodeList(여기서부터다시시작textBox.Text);
                listBox.Items.Add("종목명 저장 클릭");
                Thread rqThread = new Thread(delegate ()
                {
                    for (int i = 0; i < codeList.Count(); i++)
                    {
                        string code = codeList[i];
                        string codeName = axKHOpenAPI1.GetMasterCodeName(code);
                        using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                        {
                            // insert into table and if there is conflict update
                            //cnn.Execute("insert into freefloatData(code, codeName) values(@code, @codeName) ON CONFLICT(code) DO UPDATE SET codeName = @codeName", new { code, codeName, freefloat });
                            cnn.Execute("update freefloatData set codeName = @codeName where (code = @code)", new { code, codeName });

                        }
                    }
                });
                rqThread.Start();
                listBox.Items.Add("종목명 저장중...");
            }

        }
        private void API_OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            sw = Stopwatch.StartNew();

            // AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent 뭐 들어 있는 지 확인
            Console.WriteLine("sErrorCode: " + e.sErrorCode);
            Console.WriteLine("sMessage:" + e.sMessage);
            Console.WriteLine("sPrevNext: " + e.sPrevNext);
            Console.WriteLine("sRecordName: " + e.sRecordName);
            Console.WriteLine("sRQName:" + e.sRQName);
            Console.WriteLine("sScrNo: " + e.sScrNo);
            Console.WriteLine("sTrCode: " + e.sTrCode);
            AxKHOpenAPI API = (AxKHOpenAPI)sender;


            int indexOfCode = Array.IndexOf(codeList, code);

            if (e.sRQName.Contains("종목별투자자기관별요청")) 
            {
                string code = e.sRQName.Split('_')[1];
                listBox.Items.Add("Worked On: 종목별투자자기관별요청 " + code);
                for (int i = 0; i < API.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int price = Math.Abs(dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "현재가")));
                    float change = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, i, "등락율"));
                    int volume = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "누적거래량"));
                    int tradingValue = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "누적거래대금"));
                    int retail = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "개인투자자"));
                    int foreigner = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "외국인투자자"));
                    int insitiute = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "기관계"));
                    int IB = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "금융투자"));
                    int insurance = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "보험"));
                    int trustFund = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "투신"));
                    int etcFinance = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "기타금융"));
                    int bank = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "은행"));
                    int pensionFund = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "연기금등"));
                    int privateFund = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "사모펀드"));
                    int country = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "국가"));
                    int etcCompany = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "기타법인"));

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
                            cnn.Execute("insert or ignore into DailyCollectedData " +
                                "(recorded_time, code, date, price, change, volume, tradingValue, retail, foreigner, insitiute, IB, insurance, trustFund, etcFinance, bank, pensionFund, privateFund, country, etcCompany) " +
                                "values(@recorded_time, @code, @date, @price, @change, @volume, @tradingValue, @retail, @foreigner, @insitiute, @IB, @insurance, @trustFund, @etcFinance, @bank, @pensionFund, @privateFund, @country, @etcCompany)", new { recorded_time, code, date, price, change, volume, tradingValue, retail, foreigner, insitiute, IB, insurance, trustFund, etcFinance, bank, pensionFund, privateFund, country, etcCompany });
                        }
                    }
                    //objAuto.Set(); // 일시정지 시킨 쓰레드 다시 시작
                }
            }
            else if (e.sRQName.Contains("프로그램일별요청"))
                
            {
                string code = e.sRQName.Split('_')[1];
                string API_name = ((AxKHOpenAPI)sender).Name;

                listBox.Items.Add("Worked On: 프로그램일별요청 " + code);


                for (int i = 0; i < API.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "일자"));
                    int program = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "프로그램순매수수량"));

                    Console.WriteLine("code" + code);
                    Console.WriteLine("program" + program);

                    using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                    {
                        //update만 해야한다 왜냐하면 어느 투자자별을 할 때 종목까지 했는지 체크 할 때를 위해서 
                        cnn.Execute("update DailyCollectedData set program = @program where (code = @code and date = @date)", new { program, code, date });
                    }
                }
                //objAuto.Set();
            }

            else if (e.sRQName.Contains("주식기본정보요청"))
            {
                string code = e.sRQName.Split('_')[1];

                listBox.Items.Add("Worked On: 주식기본정보요청 " + code);

                long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")); // 이거는 안쓰네

                int freefloat = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "유통주식"));         
                int reportingMonth = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "결산월"));
                float faceValue = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "액면가")); //액면가가 소수점인 것도 있다
                int netAsset = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "자본금"));
                int numberOfStocks = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "상장주식"));
                float marginRatio = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "신용비율"));
                int annualHigh = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "연중최고"));
                int annualLow = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "연중최저"));
                int marketCap = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "시가총액"));
                float foreignerRatio = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "외인소진률"));
                int substitutePrice = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "대용가"));
                float PER = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "PER"));
                int EPS = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "EPS"));
                float ROE = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "ROE"));
                float PBR = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "PBR"));
                float EV = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "EV"));
                float BPS = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "PBR"));
                int Revenue = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "매출액"));
                int Profit = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "영업이익"));
                int Earning = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "당기순이익"));
                int twoFiveZeroHigh = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최고"));
                int twoFiveZeroLow = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최저"));
                int twoFiveZeroHighDate = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최고가일"));
                float twoFiveZeroHighTodayRatio = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최고가대비율"));
                int twoFiveZeroLowDate = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최거가일"));
                float twoFiveZeroLowTodayRatio = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "250최저가대비율"));
                int price = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "현재가"));
                float change = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "등락율"));
                int volume = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, 0, "거래량"));
                float todayYesterdayVolume = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "거래대비"));
                float freefloatRatio = dataCleansingFloat(API.GetCommData(e.sTrCode, e.sRQName, 0, "유통비율"));

                Console.WriteLine("종목코드" + code);
                Console.WriteLine("결산월" + reportingMonth);
                Console.WriteLine("액면가" + faceValue);
                Console.WriteLine("자본금" + netAsset);
                Console.WriteLine("상장주식" + numberOfStocks);
                Console.WriteLine("신용비율" + marginRatio);
                Console.WriteLine("연중최고" + annualHigh);
                Console.WriteLine("연중최저" + annualLow);
                Console.WriteLine("시가총액" + marketCap);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);
                //Console.WriteLine("유통주식수" + freefloat);

                using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                {
                    // insert into table and if there is conflict update
                    cnn.Execute("insert into freefloatData(code,freefloat,reportingMonth,faceValue,netAsset,numberOfStocks,marginRatio,annualHigh,annualLow,marketCap,foreignerRatio,substitutePrice,PER,EPS,ROE,PBR,EV,BPS,Revenue,Profit,Earning,twoFiveZeroHigh,twoFiveZeroHighDate,twoFiveZeroHighTodayRatio,twoFiveZeroLow,twoFiveZeroLowDate,twoFiveZeroLowTodayRatio,price,change,volume,todayYesterdayVolume,freefloatRatio) " +
                        "values(@code,@freefloat,@reportingMonth,@faceValue,@netAsset,@numberOfStocks,@marginRatio,@annualHigh,@annualLow,@marketCap,@foreignerRatio,@substitutePrice,@PER,@EPS,@ROE,@PBR,@EV,@BPS,@Revenue,@Profit,@Earning,@twoFiveZeroHigh,@twoFiveZeroHighDate,@twoFiveZeroHighTodayRatio,@twoFiveZeroLow,@twoFiveZeroLowDate,@twoFiveZeroLowTodayRatio,@price,@change,@volume,@todayYesterdayVolume,@freefloatRatio) " +
                        "ON CONFLICT(code) DO UPDATE SET " +
                        "freefloat=@freefloat," +
                        "reportingMonth=@reportingMonth," +
                        "faceValue=@faceValue," +
                        "netAsset=@netAsset," +
                        "numberOfStocks=@numberOfStocks," +
                        "marginRatio=@marginRatio," +
                        "annualHigh=@annualHigh," +
                        "annualLow=@annualLow," +
                        "marketCap=@marketCap," +
                        "foreignerRatio=@foreignerRatio," +
                        "substitutePrice=@substitutePrice," +
                        "PER=@PER," +
                        "EPS=@EPS," +
                        "ROE=@ROE," +
                        "PBR=@PBR," +
                        "EV=@EV," +
                        "BPS=@BPS," +
                        "Revenue=@Revenue," +
                        "Profit=@Profit," +
                        "Earning=@Earning," +
                        "twoFiveZeroHigh=@twoFiveZeroHigh," +
                        "twoFiveZeroHighDate=@twoFiveZeroHighDate," +
                        "twoFiveZeroHighTodayRatio=@twoFiveZeroHighTodayRatio," +
                        "twoFiveZeroLow=@twoFiveZeroLow," +
                        "twoFiveZeroLowDate=@twoFiveZeroLowDate," +
                        "twoFiveZeroLowTodayRatio=@twoFiveZeroLowTodayRatio," +
                        "price=@price," +
                        "change=@change," +
                        "volume=@volume," +
                        "todayYesterdayVolume=@todayYesterdayVolume," +
                        "freefloatRatio=@freefloatRatio", new {
                            code,
                            freefloat,
                            reportingMonth,
                            faceValue,
                            netAsset,
                            numberOfStocks,
                            marginRatio,
                            annualHigh,
                            annualLow,
                            marketCap,
                            foreignerRatio,
                            substitutePrice,
                            PER,
                            EPS,
                            ROE,
                            PBR,
                            EV,
                            BPS,
                            Revenue,
                            Profit,
                            Earning,
                            twoFiveZeroHigh,
                            twoFiveZeroHighDate,
                            twoFiveZeroHighTodayRatio,
                            twoFiveZeroLow,
                            twoFiveZeroLowDate,
                            twoFiveZeroLowTodayRatio,
                            price,
                            change,
                            volume,
                            todayYesterdayVolume,
                            freefloatRatio
                        });

                }
                //objAuto.Set();
            }
            else if (e.sRQName.Contains("주식일주월시분요청"))
            {
                string code = e.sRQName.Split('_')[1];

                listBox.Items.Add("Worked On: 주식일주월시분요청 " + code);
                for (int i = 0; i < API.GetRepeatCnt(e.sTrCode, e.sRQName); i++)
                {
                    long recorded_time = Int64.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
                    int date = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "날짜"));
                    int open = Math.Abs(dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "시가")));
                    int high = Math.Abs(dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "고가")));
                    int low = Math.Abs(dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "저가")));
                    int close = Math.Abs(dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "종가")));
                    int volume = dataCleansing(API.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));

                    Console.WriteLine("code" + code);
                    Console.WriteLine("close" + close);

                    using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
                    {
                        //이미 primary key가 있으면 ignore하고 아니면 insert하셈, 여기는 insert해도 된다 다른 테이블 이기 때문에
                        cnn.Execute("insert or ignore into DailyPriceInfo (recorded_time, code, date, open, high, low, close, volume) values (@recorded_time, @code, @date, @open, @high, @low, @close, @volume)", new { recorded_time, code, date, open, high, low, close, volume });

                    }
                }
                //objAuto.Set();
            }

            // 키움에서 api 얼마나 빨리 받고 프로그램에서 처리하나 계산 필요한 이유는 총 걸리는 계산 하기 위해
            sw.Stop();
            long timeLeft = (codeListCount - indexOfCode) * sw.ElapsedMilliseconds * 10; //왜 10 곱해야지 정확한게 나오는게 모르겠지만 일단 되니깐 ㄱㄱ
            TimeSpan ts = TimeSpan.FromMilliseconds(timeLeft);
            progressLabel.Text = "남은시간(hh:mm:ss) " + ts.ToString(@"hh\:mm\:ss") + Environment.NewLine + (indexOfCode + 1) + "/" + codeListCount;
            progressBar.Increment(1);
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
                if (s.Length > 0) return Int32.Parse(s);
                else return 0;
            }
            else
            {
                return 0;
            }
        }

        private float dataCleansingFloat(string s)
        {
            if (s.Length > 0)
            {
                s = s.Trim();
                if (s.Contains("+"))
                {
                    s = s.Replace("+", "");
                }
                if (s.Length > 0) return float.Parse(s);
                else return 0;
            }
            else
            {
                return 0;
            }
        }
        

        private string[] getCodeList(string interruptedCode="")
        {
            /*
             코드리스트를 받아 정리한다. 필요한 이유는 프로그램 정지 될 때, 정지된 종목부터 재실행 하기 위해 필요함
             */ 
            string kosdaq = axKHOpenAPI1.GetCodeListByMarket("10"); // 마켓에 해당하는 종목 가져오기
            string kospi = axKHOpenAPI1.GetCodeListByMarket("0"); // 마켓에 해당하는 종목 가져오기

            string[] codeArray = (kosdaq + kospi).Split(';');

            // 마켓에 해당하는 종목 가져오기

            codeArray = codeArray.OrderBy(x => x).ToArray(); // Ascending나열 해서 프로그램 정지시킨 종목을 찾기를 위함
            codeArray = codeArray.Skip(1).ToArray(); // remove first element, which is blank

            //System.IO.File.WriteAllLines("codeArray.txt", codeArray.Select(tb => tb));





            if (interruptedCode.Equals("") == false)
            {
                int index = Array.FindIndex(codeArray, row => row == interruptedCode);
                string[] segment = codeArray.Skip(index).Take(codeArray.Count()).ToArray(); // 와 이거 하는데 정말 오래걸랬다. 근데 정확한것도 아님... [index..] 이것도 안먹음 python은 그냥 array[index:] 이러면 끝인데...
                codeListCount = segment.Count();
                codeArray = segment;
            }
            else
            {
                codeListCount = codeArray.Count();
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

 
            if (screenNum >= 1199)
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


        private void CheckDataExist(string button)
        {
            codeList = getCodeList(여기서부터다시시작textBox.Text);
            using (IDbConnection cnn = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Default"].ConnectionString))
            {
                string date = dateTextBox.Text;
                string fromHere = "";
                if(button.Equals("종목별투자자기관별요청"))
                {
                    for (int i = 0; i < codeListCount; i++)
                    {
                        code = codeList[i];
                        var exists = cnn.ExecuteScalar<bool>("select count(1) from DailyCollectedData where (date=@date and code=@code)", new { date, code });
                        if (exists)
                        {
                            fromHere = code;
                        }
                    }
                }
                else if(button.Equals("프로그램일별요청"))
                {
                    if (여기서부터다시시작textBox.Text!="")
                    {
                        fromHere = 여기서부터다시시작textBox.Text;
                    }
                    else
                    {
                        for (int i = 0; i < codeListCount; i++)
                        {
                            code = codeList[i];
                            var programQuantity = cnn.ExecuteScalar("select program from DailyCollectedData where (date=@date and code=@code)", new { date, code });
                            if (programQuantity == null)
                            {
                                fromHere = code;
                            }
                        }
                    }
                 
                }
                else if(button.Equals("주식일주월시분요청"))
                {
                    for (int i = 0; i < codeListCount; i++)
                    {
                        code = codeList[i];
                        var exists = cnn.ExecuteScalar<bool>("select count(1) from DailyPriceInfo where (date=@date and code=@code)", new { date, code });
                        if (exists)
                        {
                            fromHere = code;
                        }
                    }
                }


                여기서부터다시시작textBox.Text = fromHere;




                //else if (button.Equals("주식기본정보요청"))
                //    // 이건 불필요 왜냐하면 이미 있는 데이터 위에 덮어 쓰는거니깐
                //{
                //    for (int i = 0; i < codeListCount; i++)
                //    {
                //        code = codeList[i];
                //        var exists = cnn.ExecuteScalar<bool>("select count(1) from freefloatData where code=@code", new { code });             
                //        if (exists)
                //        {
                //            fromHere = code;
                //        }
                //    }
                //}


            }
            codeList = getCodeList(여기서부터다시시작textBox.Text);
            progressBar.Maximum = codeList.Count();
        }
    }
}
