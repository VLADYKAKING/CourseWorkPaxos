using CourseWorkPaxos;
using System.Windows;
using System.Windows.Input;

namespace paxos
{
    public partial class MainWindow : Window
    {
        Paxos paxos;
        public MainWindow()
        {
            InitializeComponent();
            paxos = new Paxos();
            paxos.Init(MainCanvas, txtStatus);


        }
        bool MouseDownOdd = false;
        private void MouseDown(object sender, MouseEventArgs e)
        {
            MouseDownOdd = !MouseDownOdd;
            if (MouseDownOdd)
            {
                paxos.Stop();
            }
            else
            {
                paxos.Resume();
            }
        }

    }
}
