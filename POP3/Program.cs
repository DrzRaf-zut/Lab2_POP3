using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POP3
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                POP3Client pop3Client = new POP3Client();
                pop3Client.runClient();
                Console.WriteLine("Klient POP3 rozpoczal dzialanie. Aby zakończyć program, wprowadź 'q'\n");

                char c;
                while ((c = Console.ReadKey(true).KeyChar) != 'q')
                    Console.WriteLine("Podana komenda jest bledna. Wprowadz 'q' by zakonczyc program.\n");

                pop3Client.stopClient();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
