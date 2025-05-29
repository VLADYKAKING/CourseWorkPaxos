using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CourseWorkPaxos
{
    public class Paxos
    {
        //use to draw ui
        Canvas canvas;
        //use to print status text
        TextBlock txtStatus;
        //all character
        public List<Shape> rms = new List<Shape>();
        //every era's determined data{era,data}
        private Dictionary<int, int> eraData = new Dictionary<int, int>();
        //if determined data has conflication, store them{era,error message}
        private Dictionary<int, List<object>> errorEraMsg = new Dictionary<int, List<object>>();
        //every port to the shape entity
        private Dictionary<int, Shape> portToRMDict = new Dictionary<int, Shape>();

        public int GetEraValue(int era)
        {
            return eraData[era];
        }

        public static int ArrowThinessTimeSlide()
        {
            return 2;
        }

        public static int ArrowMaxThiness()
        {
            return 5;
        }

        public int GetAllRmCount()
        {
            return portToRMDict.Count;
        }

        public List<RM> GetAllRM()
        {
            List<RM> res = new List<RM>();
            foreach (var it in portToRMDict)
            {
                if (it.Value.Tag is RM)
                {
                    res.Add(it.Value.Tag as RM);
                }
            }
            return res;
        }

        int[] proposeCnt = new int[10];
        Dictionary<int, int> usedMsgId = new Dictionary<int, int>();

        public void DeterminEraMsg(int era, int value, RM rm)
        {
            if (eraData.ContainsKey(era))
            {
                if (eraData[era] != value)
                {
                    if (!errorEraMsg.ContainsKey(era))
                    {
                        errorEraMsg.Add(era, new List<object>());
                        Logger.Error("error:era have different value,pre :" + eraData[era] + " after: " + value);
                    }
                    errorEraMsg[era].Add(value);
                }
                else
                {
                    Logger.Log("duplicate determine era " + era + " value " + value);
                }
            }
            else
            {
                List<Proposer> proposers = rm.parent.GetAllProposer();
                int flag = 0;
                foreach (Proposer p in proposers)
                {
                    if (p.proposeValue == value)
                    {
                        Logger.Log("Proposer " + p.id + " propose era " + era + " value " + value + " and rm " + rm.id + " determine");
                        flag = 1;
                        if (p.id != rm.id)
                        {
                            Logger.Log("a succed rm receice the preview value");
                        }
                    }
                }
                eraData[era] = value;
                if (flag == 0)
                {
                    Logger.Error("a not proposed value is propose");
                }
                proposeCnt[rm.id]++;
                if (!usedMsgId.ContainsKey(rm.state.sendedMsgId))
                {
                    usedMsgId[rm.state.sendedMsgId] = 0;
                }
                usedMsgId[rm.state.sendedMsgId]++;
            }
        }

        public RM PortToRM(int port)
        {
            return portToRMDict[port].Tag as RM;
        }

        public long GetWaitWorkMaxTime()
        {
            return 6;
        }

        public long GetWait1bMaxTime()
        {
            return 6;
        }

        public long GetWait2bMaxTime()
        {
            return 8;
        }

        //public List<int> getAllAcceptor()
        //{
        //    List<int> res = new List<int>();
        //    foreach (var it in portToRMDict)
        //    {
        //        if (it.Value.Tag is Acceptor)
        //        {
        //            res.Add(it.Key);
        //        }
        //    }
        //    return res;
        //}

        public List<Acceptor> GetAllAcceptor()
        {
            List<Acceptor> res = new List<Acceptor>();
            foreach (var it in portToRMDict)
            {
                if (it.Value.Tag is Acceptor)
                {
                    res.Add(it.Value.Tag as Acceptor);
                }
            }
            return res;
        }


        public List<Proposer> GetAllProposer()
        {
            List<Proposer> res = new List<Proposer>();
            foreach (var it in portToRMDict)
            {
                if (it.Value.Tag is Proposer)
                {
                    res.Add(it.Value.Tag as Proposer);
                }
            }
            return res;
        }

        public Acceptor GetRandAcceptor()
        {
            List<Acceptor> acceptors = GetAllAcceptor();
            return acceptors[Util.rnd.Next(0, acceptors.Count)];
        }

        bool stopped = false;

        public void Init(Canvas canvas, TextBlock txtStatus)
        {
            //init data
            this.canvas = canvas;
            this.txtStatus = txtStatus;
            int id = 0;
            int max = 6;//determine all rm count
            double radiu = 200;//round of ui size
            double centerX = 250, centerY = 250;//ui center point
            for (int i = 0; i < max; i += 1, id += 1)
            {
                Shape shape;
                RM rm;
                if (id < max / 2)
                {
                    rm = new Acceptor();
                    rm.parent = this;
                    rm.id = id;
                    rm.x = (int)(centerX + radiu * Math.Cos(2 * Math.PI * (double)id / max));
                    rm.y = (int)(centerY + radiu * Math.Sin(2 * Math.PI * (double)id / max));

                    rm.rgb[0] = rm.rgb[1] = rm.rgb[2] = 100;
                    shape = new Ellipse()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Color.FromRgb(rm.rgb[0], rm.rgb[1], rm.rgb[2])),
                        StrokeThickness = 6
                    };
                    shape.Tag = rm;
                }
                else
                {
                    rm = new Proposer();
                    rm.parent = this;
                    rm.id = id;
                    rm.x = (int)(centerX + radiu * Math.Cos(2 * Math.PI * (double)id / max));
                    rm.y = (int)(centerY + radiu * Math.Sin(2 * Math.PI * (double)id / max));

                    rm.rgb[0] = rm.rgb[1] = rm.rgb[2] = 200;
                    shape = new Rectangle()
                    {
                        Width = 20,
                        Height = 20,
                        Stroke = new SolidColorBrush(Color.FromRgb(rm.rgb[0], rm.rgb[1], rm.rgb[2])),
                        StrokeThickness = 6
                    };
                    shape.Tag = rm;
                }
                canvas.Children.Add(shape);
                rms.Add(shape);
                portToRMDict.Add((shape.Tag as RM).connector.localPort, shape);
                shape.SetValue(Canvas.LeftProperty, (double)rm.x);
                shape.SetValue(Canvas.TopProperty, (double)rm.y);
            }

            if (Util.DisplayUI())
            {
                //timer start
                System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                dispatcherTimer.Tick += PaxosTimer;
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                dispatcherTimer.Start();
                Connector.StartConsumeThread();
            }
            else
            {
                Console();
                Thread thread = new Thread(new ThreadStart(Console));
                thread.Start();
            }

        }

        void Console()
        {
            for (; ; )
            {
                curTime++;
                PaxosAct();
                Connector.ConsumeCurrent();
            }
        }

        public void Stop()
        {
            stopped = true;
        }

        public void Resume()
        {
            stopped = false;
        }

        public void PaxosAct()
        {
            List<RM> rm = new List<RM>();
            foreach (Shape i in rms)
            {
                rm.Add((i.Tag as RM));
            }
            foreach (RM i in rm)
            {
                i.Act();
            }
        }

        static long curTime = 0;
        static public long GetTime()
        {
            return curTime;
        }

        private void PaxosTimer(object sender, EventArgs e)
        {
            if (stopped)
                return;

            curTime++;
            PaxosAct();
            Render.render(canvas.Children, canvas);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Состояние узлов Paxos ===");

            foreach (UIElement it in canvas.Children)
            {
                if (it is Ellipse || it is Rectangle)
                {
                    var shape = it as Shape;
                    if (!string.IsNullOrEmpty(shape.Name))
                        continue;

                    RM rm = shape.Tag as RM;
                    string type = rm is Acceptor ? "Acceptor" : "Proposer";
                    sb.AppendLine($"{type} [ID: {rm.id}] — Состояние: {rm.state.GetType().Name}");

                    if (rm is Acceptor && rm.state is AcceptorState acceptor)
                    {
                        sb.AppendLine($"  ↳ 1a получен: {acceptor.received1aMsgId}, 2a отправлен: {acceptor.sended2bMsgId}, maxID: {acceptor.participateMaxId}, Эпоха: {rm.curEra}");
                    }
                    else if (rm is Proposer && rm.state is ProposerState proposer)
                    {
                        sb.AppendLine($"  ↳ Последнее сообщение: ID={proposer.sendedMsgId}, Значение={((Proposer)rm).proposeValue}, Эпоха: {rm.curEra}");
                    }

                    sb.AppendLine(); // Пустая строка между узлами
                }
            }

            txtStatus.Text = sb.ToString();
        }

    }
}
