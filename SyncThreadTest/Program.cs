using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyncThreadTest
{
    class Producer
    {
        private int num;
        public static int maxNum = 3;
        private Thread t;
        private CommonData _d;
        public int Num
        {
            get { return num;}
            set { num = Math.Abs(value) % maxNum; }
        }
        public Producer(CommonData d, int num)
        {
            _d = d;
            Num = num;
            Start();
        }

        private void Generate()
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            int delay = r.Next(1000, 5000);
            Thread.Sleep(delay);
            int result = r.Next(0, 100);
            Monitor.Enter(_d);
            _d.Set(Num, result);
            Monitor.Pulse(_d);
            Monitor.Exit(_d);
            Console.WriteLine("Производитель создал число №{0} = {1}", Num, result);
        }
        public void Start()
        {
            if (t == null || !t.IsAlive)
            {
                ThreadStart th = new ThreadStart(Generate);
                t = new Thread(th);
                t.Start();
            }
        }
    }

    class Consumer
    {
        private Thread t;
        private CommonData _d;
        public Consumer(CommonData d)
        {
            _d = d;
            Start();
        }

        private void Get()
        {
            Monitor.Enter(_d);
            int[] result = null;
            try
            {
                while (_d.Filled < 3)
                {
                    Monitor.Wait(_d);
                }
                result = _d.Get();
            }
            catch (Exception e)
            {
            }
            finally
            {
                Monitor.Exit(_d);
            }

            if (result != null)
            {
                int d = 0;
                for (int i = 0; i < Producer.maxNum; i++)
                {
                    d += result[i];
                }

                Console.WriteLine("Результат потребителя: {0}", d);
            }
        }

        public void Start()
        {
            if (t==null || !t.IsAlive)
            {
                ThreadStart th = new ThreadStart(Get);
                t = new Thread(th);
                t.Start();
            }
        }
    }

    class CommonData
    {
        int [] _results;
        public int Filled { get; private set; }
        public CommonData()
        {
            _results = new int[3];
            Filled = 0;
        }

        public void Set(int index, int value)
        {
            _results[index] = value;
            Filled++;
        }

        public int[] Get()
        {
            Filled = 0;
            return _results;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            CommonData cd = new CommonData();
            for (int i = 0; i < Producer.maxNum; i++)
            {
                new Producer(cd, i);
            }
            new Consumer(cd);
            Console.ReadKey();
        }
    }
}
