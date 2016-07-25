using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
// Add these two statements to all SimConnect clients
using Microsoft.FlightSimulator.SimConnect;
using System.Runtime.InteropServices;

namespace SimuladorArduino
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
            buscarPortaSerial();
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

        void buscarPortaSerial()
        {
            String[] ports = SerialPort.GetPortNames();
            comboBox1.Items.AddRange(ports);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
                {
                //serialPort1.PortName = "COM4";
                serialPort1.PortName = comboBox1.Text;
                serialPort1.Open();
                serialPort1.BaudRate = 9600;
                button1.Enabled = false;
                button2.Enabled = true;
                richTextBox1.ReadOnly = true;
                richTextBox2.ReadOnly = true;
                //timer1.Start();
                }
            catch
                {
                //MessageBox.Show("Erro ao conectar, verifique se o modulo esta ligado na USB",

                MessageBox.Show("Verifique se o modulo esta ligado na USB", "Erro ao conectar!",
     MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

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

                    // P é o movimento do profundor pitch
                    if (s1.a >= 0)
                    {
                        TextBox1("P" + s1.a);
                        serialPort1.WriteLine("P"+ s1.a);
                    }

                    if (s1.a <= 0)
                    {
                        TextBox1("P" + s1.a);
                        serialPort1.WriteLine("P" + s1.a);
                    }

                    // A é o movimento do aileron pitch
                    if (s1.b >= 0)
                    {
                        TextBox2("A" + s1.b);
                        serialPort1.WriteLine("A" + s1.b);
                    }

                    if (s1.b <= 0)
                    {
                        TextBox2("A" + s1.b);
                        serialPort1.WriteLine("A" + s1.b);
                    }
                   
                    break;

                default:
                    displayText("Unknown request ID: " + data.dwRequestID);
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
            serialPort1.Close();
            button1.Enabled = true;
            closeConnection();
            //button2.Enabled = false;
            //textBox1.ReadOnly = true;
            //textBox1.Clear();
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
            richTextBox3.Text = output;
        }

        void TextBox1(string s)
        {
            richTextBox1.Text = s;
        }

        void TextBox2(string s)
        {
            richTextBox2.Text = s;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Stop();
            serialPort1.Close();
            button1.Enabled = true;
            closeConnection();
        }
    }
}
