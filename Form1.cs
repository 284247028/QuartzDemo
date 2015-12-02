using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuartzDemo
{
    using Quartz;
    using Quartz.Impl;
    using Quartz.Job;
    using System.Threading;

    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    public partial class Form1 : Form
    {
        private string _msg = "";
        //从StdSchedulerFactory获取 Scheduler实例 
        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
        public Form1()
        {
            InitializeComponent();
        }
        private bool isFirst = true;
        public Form1(string msg)
        {
            InitializeComponent();
            _msg = msg;
            try
            {
                //启动 scheduler
                scheduler.Start();
                // 定义一个job并和自定义的HelloJob进行绑定
                IJobDetail job = JobBuilder.Create<HelloJob>()
                    .WithIdentity("HelloJob1", "SimpleGroup1")
                    .Build();
                #region Cron Expressions 每隔1秒
                ITrigger trigger2 = TriggerBuilder.Create()
               .WithIdentity("trigger3", "SimpleGroup")
               .WithCronSchedule("0/1 * * * * ?", x => x
                   .WithMisfireHandlingInstructionFireAndProceed())
               .ForJob("HelloJob1", "SimpleGroup1")
               .Build();
                #endregion

                //定义一个即时触发的触发器,（每隔1秒进行重复执行）
                ITrigger trigger= TriggerBuilder.Create()
                    .WithIdentity("trigger1", "SimpleGroup")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(1)
                        .RepeatForever())
                    .Build();
                // 将job和trigger进行绑定，并告知 quartz 调度器用trigger去执行job 
                //scheduler.ScheduleJob(job, trigger);
                scheduler.ScheduleJob(job, trigger2);
            }
            catch (SchedulerException se)
            {
                Console.WriteLine(se);
            }
       
        }
        public OxyPlot.WindowsForms.PlotView  Plot;
        private LineSeries lineSeries = new LineSeries { Title = "即时监控（1秒）", StrokeThickness =2 };
        private void Form1_Load(object sender, EventArgs e)
        {
            Plot = new OxyPlot.WindowsForms.PlotView();
            Plot.Model = new PlotModel();
            Plot.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(Plot);

            Plot.Model.PlotType = PlotType.XY;
            Plot.Model.Background = OxyColor.FromRgb(255, 255, 255);
            Plot.Model.TextColor = OxyColor.FromRgb(0, 0, 0);
            
            // add Series and Axis to plot model
            Plot.Model.Series.Add(lineSeries);
            Plot.Model.Axes.Add(new LinearAxis());
        }
        private double xInit = 0;
        public bool SetMsg(string msg)
        {
            _msg = msg;
            //号称NET4最简单的跨进程更新UI的方法
            this.Invoke((MethodInvoker)delegate
            {
                // runs on UI thread
                if (isFirst)
                {
                    this.txtLog.AppendText("Hello to Quartz NET ! Created by JackWang 2015");
                    isFirst = false;
                }
                this.txtLog.AppendText(string.Format("\r\n$JackWang>> You get {0} message from Quartz MyJob...", _msg));
                xInit = xInit + 1;
                lineSeries.Points.Add(new DataPoint(xInit,double.Parse(_msg)));
                if (lineSeries.Points.Count > 50)
                {
                    //保留最近50个点
                    lineSeries.Points.RemoveAt(0);
                }
                 //更新图表数据
                this.Plot.Model.InvalidatePlot(true);
               
            });
            return true;

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //关闭 scheduler
            scheduler.Shutdown();
        }
    }
}
