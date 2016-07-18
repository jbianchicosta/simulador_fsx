using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

// Add these two statements to all SimConnect clients
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;
using System.Collections;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        // User-defined win32 event
        const int WM_USER_SIMCONNECT = 0x0402;

        // SimConnect object
        SimConnect simconnect = null;

        enum DEFINITIONS
        {
            Struct1,
        }

        enum DATA_REQUESTS
        {
            REQUEST_1,
        };

        // this is how you declare a data structure so that
        // simconnect knows how to fill it/read it.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        struct Struct1
        {
            // this is how you declare a fixed size string
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            //public String title;
            //public double latitude;
            //public double longitude;
            //public double altitude;
            //public int y;
            public int a;
            public int b;
            //public int abc;
        };

        public Form1()
        {
            InitializeComponent();
        }

        // Simconnect client will send a win32 message when there is 
        // a packet to process. ReceiveMessage must be called to
        // trigger the events. This model keeps simconnect processing on the main thread.
        // Usado pela API para criar conexao no FSX
        protected override void DefWndProc(ref Message m)
        {
            if (m.Msg == WM_USER_SIMCONNECT)
            {
                if (simconnect != null)
                {
                    simconnect.ReceiveMessage();
                }
            }
            else
            {
                base.DefWndProc(ref m);
            }
        }

        private void closeConnection()
        {
            if (simconnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                simconnect.Dispose();
                simconnect = null;
                displayText("Conexao fechada");
            }
        }

        // Set up all the SimConnect related data definitions and event handlers
        // Recebe todoas as variaveis da aeronave e guarda em suas variaveis
        private void initDataRequest()
        {
            try
            {
                // listen to connect and quit msgs
                simconnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(simconnect_OnRecvOpen);
                simconnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(simconnect_OnRecvQuit);

                // listen to exceptions
                simconnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(simconnect_OnRecvException);

                // define a data structure
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "Title", null, SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Latitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Longitude", "degrees", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "Plane Altitude", "feet", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "YOKE Y POSITION", null, SIMCONNECT_DATATYPE.FLOAT64, 2.1f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "YOKE X POSITION", null, SIMCONNECT_DATATYPE.FLOAT64, 2.1f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "YOKE Z POSITION", null, SIMCONNECT_DATATYPE.FLOAT64, 2.1f, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "ATTITUDE INDICATOR BANK DEGREES", "Degrees", SIMCONNECT_DATATYPE.FLOAT64, 0, SimConnect.SIMCONNECT_UNUSED);
                //simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "ATTITUDE INDICATOR PITCH DEGREES", "Degrees", SIMCONNECT_DATATYPE.INT64, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "PLANE PITCH DEGREES", "degrees", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                simconnect.AddToDataDefinition(DEFINITIONS.Struct1, "PLANE BANK DEGREES", "degrees", SIMCONNECT_DATATYPE.INT32, 0, SimConnect.SIMCONNECT_UNUSED);
                
                // IMPORTANT: register it with the simconnect managed wrapper marshaller
                // if you skip this step, you will only receive a uint in the .dwData field.
                simconnect.RegisterDataDefineStruct<Struct1>(DEFINITIONS.Struct1);

                // catch a simobject data request
                simconnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(simconnect_OnRecvSimobjectDataBytype);
            }
            catch (COMException ex)
            {
                displayText(ex.Message);
            }
        }

        void simconnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            displayText("Conectedo ao FSX");
        }

        // caso o usuario fechar o FSX
        void simconnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            displayText("FSX foi fechado");
            closeConnection();
        }
        // tratar erros
        void simconnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            displayText("Exception received: " + data.dwException);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (simconnect == null)
            {
                try
                {
                  // the constructor is similar to SimConnect_Open in the native API
                  simconnect = new SimConnect("Managed Data Request", this.Handle, WM_USER_SIMCONNECT, null, 0);

                  initDataRequest();
                  timer1.Start();
                }
                catch (COMException ex)
                {
                    displayText("Nao foi possivel conectar ao FSX");
                }
            }
            else
            {
                displayText("Error - try again");
                closeConnection();

             //   setButtons(true, false, false);
            }
        }
               

        void simconnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            switch ((DATA_REQUESTS)data.dwRequestID)
            {
                case DATA_REQUESTS.REQUEST_1:
                    Struct1 s1 = (Struct1)data.dwData[0]; //  monta a string s1 para enviar aos textbox

                    // displayText("Title: " + s1.title);
                    //displayText("Lat:   " + s1.latitude);
                    //displayText("Lon:   " + s1.longitude);
                    //displayText("Alt:   " + s1.altitude);

                    //TextBox1("" + s1.x);
                    //TextBox2("" + s1.x);
                    //TextBox3("" + s1.y);
                    //TextBox3("B" + s1.b);

                    // A é o movimento do profundor pitch
                    if (s1.a >= 0)
                    {
                      TextBox1("A" + s1.a);
                    }

                    if (s1.a <= 0)
                    {
                        TextBox4("A" + s1.a);
                    }

                    // B é o movimento do profundor pitch
                    if (s1.b >= 0)
                    {
                        TextBox2("B" + s1.b);
                    }

                    if (s1.b <= 0)
                    {
                        TextBox3("B" + s1.b);
                    }
                    //TextBox6("" + s1.altitude);
                    //TextBox7("" + s1.velocidade);


                    break;

                default:
                    displayText("Unknown request ID: " + data.dwRequestID);
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (simconnect != null)
            {
                // Dispose serves the same purpose as SimConnect_Close()
                simconnect.Dispose();
                simconnect = null;
                //displayText("Connection closed");
                timer1.Stop();
                closeConnection();
                displayText("Conexao fechada");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // The following call returns identical information to:
             //simconnect.RequestDataOnSimObject(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, SimConnect.SIMCONNECT_OBJECT_ID_USER, SIMCONNECT_PERIOD.ONCE);
             try
            {
                simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, 1, SIMCONNECT_SIMOBJECT_TYPE.USER);
            }
            catch
            {
                displayText("Erro ja conectado!");
                timer1.Stop();
            }
            //displayText("Request sent...");
        }

        // Response number
        int response = 1;

        // Output text - display a maximum of 10 lines
        string output = "\n";  // \n para jogar o texto abaixo de 10 linhas

        void displayText(string s)
        {
            // remove first string from output
            output = output.Substring(output.IndexOf("\n") + 1);

            // add the new string
            output += "" + response++ + ": " + s;

            // display it
            richTextBox6.Text = output;
        }

        void TextBox1(string s)
        {
            richTextBox1.Text = s;
        }

        void TextBox2(string s)
        {
            richTextBox2.Text = s;
        }

       void TextBox3(string s)
        {
            richTextBox3.Text = s;
        }

        void TextBox4(string s)
        {
            richTextBox4.Text = s;
        }

        void TextBox6(string s)
        {
            // remove first string from output
            // output = output.Substring(output.IndexOf("\n"));

            // add the new string
            //output += "\n" + response++ + ": " + s;
            //output = s;

            // mostra os dados
            richTextBox6.Text = s;

            /*ArrayList linhas = new ArrayList(richTextBox6.Lines);
            // ordena as linhas
            linhas.Sort();
            //richTextBox6.Clear();
            richTextBox6.Lines = (String[])linhas.ToArray(typeof(string));*/
        }
        void TextBox7(string s)
        {
            richTextBox7.Text = s;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            simconnect.RequestDataOnSimObjectType(DATA_REQUESTS.REQUEST_1, DEFINITIONS.Struct1, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);
            displayText("Request sent...");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closeConnection();
        }
    }
}
