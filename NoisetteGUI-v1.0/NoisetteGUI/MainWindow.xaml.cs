using dnlib.DotNet;
using dnlib.DotNet.Writer;
//using Noisette;
using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using NoisetteGUI.Protection.Anti;
using NoisetteGUI.Protection.ControlFlow;
using NoisetteGUI.Protection.AddInt;
using NoisetteGUI.Protection.Renamer;
using NoisetteGUI.Protection.String;
//using NoisetteGUI.Protection.StringOnline;
using NoisetteGUI.Protection.StactProtection;

namespace NoisetteGUI
{
    public partial class MainWindow : Window
    {
        BackgroundWorker bgWorker = new BackgroundWorker();
        public static string _file;

        public static MethodDef Init;
        public static MethodDef Init2;

        Storyboard sb;
        int error;
        string errorMG;

        public MainWindow()
        {
            InitializeComponent();
            bgWorker.DoWork +=
         new DoWorkEventHandler(bgWorker_DoWork);

            bgWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            bgWorker.WorkerReportsProgress = true;

            sb = this.FindResource("Rct_white_storyB") as Storyboard;

        }

        public void startanimation()
        {
           
        }
        private void ExitButton_MouseEnter(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MinimizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void MaximizeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void DragTheForm(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void textBox_Drop(object sender, DragEventArgs e)
        {

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;
            //int error = 0;
            //string errormsg = null;
            _file = files[0];
            bgWorker.RunWorkerAsync();

            this.Box_dropArea.Visibility = Visibility.Hidden;
            this.textBlock.Visibility = Visibility.Hidden;
            this.textBox.Visibility = Visibility.Hidden;

            sb.Begin();
            
        }

        public void OnDragOver(object sender, DragEventArgs e)

        {
            e.Effects = DragDropEffects.All;

            e.Handled = true;
        }

        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (error != 0)
            {
                EndWindow end = new NoisetteGUI.EndWindow
                {
                    NoError_btn = { Visibility = Visibility.Hidden },
                    NoError_txt = { Visibility = Visibility.Hidden },
                    Error_btn = { Visibility = Visibility.Visible },
                    Error_txt = { Visibility = Visibility.Visible }
                };
                end.LogTXT.Document.Blocks.Clear();
                end.LogTXT.AppendText(errorMG);
                end.Show();
                this.Hide();
            }
            else
            {
                EndWindow end = new NoisetteGUI.EndWindow();
                end.LogTXT.Document.Blocks.Clear();
                end.LogTXT.AppendText("All is ok apparently... :)");
                end.Show();
                this.Hide();
            }
        }


        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ModuleContext modCtx = ModuleDef.CreateModuleContext();
                var module = ModuleDefMD.Load(_file, modCtx);
                //encypt string
                StringEncPhase.Execute(module);
                //可扩展部分，自定义一个url发布解密后的字符串。该部分原理将字符串加密，然后用加密后的字符串去下载解密后的字符串并完成替换。
                //OnlinePhase.Execute(module);
                //更改控制流
                ControlFlowObfuscation.Execute(module);
                //插入INT混淆
                AddIntPhase.Executes(module);
                //更改程序流
                ControlFlowObfuscation.Execute(module);
                //栈混淆
                StackUnfConfusion.Execute(module);
                //对.net程序的type，field，Methods进改名操作
                RenamerPhase.Execute(module);
                //反编译
                AntiDe4dot.Execute(module.Assembly);
                JumpCFlow.Execute(module);
                //Anti 防篡改
                AntiTamper.Execute(module);
                var path = $"{Path.GetFileNameWithoutExtension(_file)}_protected{Path.GetExtension(_file)}";
                //var dirpath = Path.GetDirectoryName(module.Location);
                //var exepath = Path.GetFileNameWithoutExtension(module.Location) + "_protected.exe";
                module.Write(path, new ModuleWriterOptions(module)
                { PEHeadersOptions = { NumberOfRvaAndSizes = 13 }, Logger = DummyLogger.NoThrowInstance });

                Protection.Anti.AntiTamper.Sha256(path);
            }
            catch (Exception ex)
            {
                //something went wrong
                error = 1;
                errorMG = ex.ToString();
            }
           
        }
    }
}