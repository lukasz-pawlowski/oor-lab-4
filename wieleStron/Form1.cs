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

        private void dodajOtwartaStrone()
        {
            otwarteStrony++;
            label1.Text = "Otwarte strony: " + otwarteStrony.ToString();
            tabControl1.TabPages[otwarteStrony-1].Text = tabControl1.TabPages[otwarteStrony-1].Text + " - otwarte";
        }
        private void dodajOtwieraneStrony()
        {
            otwieraneStrony++;
            label2.Text = "Otwierane strony: " + otwieraneStrony.ToString();
            tabControl1.TabPages[otwieraneStrony-1].Text = tabControl1.TabPages[otwieraneStrony-1].Text + " - ładowanie";
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
    }
}
