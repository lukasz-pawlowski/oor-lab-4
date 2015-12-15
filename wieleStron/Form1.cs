using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading;

namespace wieleStron
{

    public partial class Form1 : Form
    {
        int otwarteStrony = 0;
        int otwieraneStrony = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Task<int> returnedTaskTResult5 = AccessTheWebAsync("http://blednastrona-blednyurl.com", webBrowser5);

            Task<int> returnedTaskTResult1 = AccessTheWebAsync("https://www.google.com", webBrowser1);
            Task<int> returnedTaskTResult2 = AccessTheWebAsync("http://www.wp.pl", webBrowser2);
            Task<int> returnedTaskTResult3 = AccessTheWebAsync("http://onet.pl", webBrowser3);
            Task<int> returnedTaskTResult4 = AccessTheWebAsync("http://msdn.microsoft.com", webBrowser4);
        }



        Receive rec1 = new Receive();
        Receive rec2 = new Receive();

        private void wysylacz(object sender, EventArgs e)
        {
            // rec1.create();
            // rec2.create();
            Thread producerThread1 = new Thread(new ThreadStart(rec1.RecMain));
            Thread producerThread2 = new Thread(new ThreadStart(rec2.RecMain));
            Thread consumer = new Thread(new ThreadStart(produce));
            // rec1.RecMain();
            // rec2.RecMain();
            consumer.Start();
            producerThread1.Start();
            producerThread2.Start();



        }

        private void produce()
        {
            string argsFull = textBox1.Text;
            string[] args = argsFull.Split();

            foreach (string arg in args)
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using (var connection = factory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);


                    var body = Encoding.UTF8.GetBytes(arg);


                    channel.BasicPublish(exchange: "",
                                         routingKey: "task_queue",
                                         basicProperties: properties,
                                         body: body);

                    // richTextBox1.Text += " [x] Sent " + arg + Environment.NewLine;
                }
                Thread.Sleep(1000);
            }
        
        }
        private void dodajOtwartaStrone()
        {
            otwarteStrony++;
            label1.Text = "Otwarte strony: " + otwarteStrony.ToString();
            tabControl1.TabPages[otwarteStrony - 1].Text = tabControl1.TabPages[otwarteStrony - 1].Text + " - otwarte";
        }
        private void dodajOtwieraneStrony()
        {
            otwieraneStrony++;
            label2.Text = "Otwierane strony: " + otwieraneStrony.ToString();
            tabControl1.TabPages[otwieraneStrony - 1].Text = tabControl1.TabPages[otwieraneStrony - 1].Text + " - ładowanie";
        }

        async Task<int> AccessTheWebAsync(string strona, WebBrowser browserTab)
        {
            //klient HTTP
            HttpClient client = new HttpClient();

            // GetStringAsync returns a Task<string>. That means that when you await the
            // task you'll get a string (urlContents).
            Task<string> getStringTask = client.GetStringAsync(strona);

            //dodaj otwierane strony - licznik
            dodajOtwieraneStrony();

            //zawiesza watek - czeka na dokonczenie jego pracy przed wykonaniem dalszych instrukcji
            string urlContents = await getStringTask;

            browserTab.ScriptErrorsSuppressed = true; //wylacz bledy javascript
            browserTab.DocumentText = urlContents;

            //dodanie numeru do gotowych, otwartych stron
            dodajOtwartaStrone();

            //zwraca rozmiar strony
            return urlContents.Length;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            richTextBox2.Text = rec1.message;
            richTextBox3.Text = rec2.message;
        }

        private void button4_Click(object sender, EventArgs e)
        {


        }
    }

    class Receive
    {
        public string message;

        public void RecMain()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                this.message += " [*] Waiting" + Environment.NewLine;

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var txt = Encoding.UTF8.GetString(body);
                    //Console.WriteLine(" [x] Received {0}", message);
                    this.message += " [x] Received " + txt + Environment.NewLine;

                    Random r = new Random();
                    int f = r.Next(100);
                    Thread.Sleep(1000 + f);

                    // Console.WriteLine(" [x] Done");
                    this.message += " [x] Done" + Environment.NewLine;

                    // channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };
                channel.BasicConsume(queue: "task_queue",
                                     noAck: false,
                                     consumer: consumer);

            }
        }

    }
}
