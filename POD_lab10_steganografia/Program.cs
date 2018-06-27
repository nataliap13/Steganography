using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using System.Drawing;

namespace POD_lab10_steganografia
{
    class Point
    {
        private int x;
        private int y;
        public Point(int x_, int y_)
        {
            x = x_;
            y = y_;
        }

        public int Y { get => x; }
        public int X { get => y; }
        override public string ToString()
        {
            return string.Format("({0}, {1})", x, y);
        }
        public bool Equals(Point p)
        {
            if (X == p.X && Y == p.Y)
                return true;
            else return false;
        }
    }
    class Program
    {
        //private static string format = "{0,4:D}";
        private static Random rand;
        public static void Main()
        {
            Console.WriteLine("Autor: Natalia Popielarz");
            int choice = 1;
            while (choice != 0)
            {
                Console.WriteLine("\n\n\nMenu - Steganografia");
                Console.WriteLine("1. Zapisz wiadomosc w obrazie");
                Console.WriteLine("2. Odczytaj wiadomosc z obrazu");
                Console.WriteLine("3. Operacje na znaku wodnym");
                Console.WriteLine("0. Wyjscie");
                Console.Write("\nWybierz opcje: ");
                if (int.TryParse(Console.ReadLine(), out choice))
                {
                    switch (choice)
                    {
                        case 1:
                            {
                                Encryption_in_image();
                                break;
                            }
                        case 2:
                            {
                                Decryption_from_image();
                                break;
                            }
                        case 3:
                            {
                                Watermark();
                                break;
                            }
                        default:
                            break;
                    }
                }
                else
                {
                    choice = 1;
                }
            }
        }

        public static void Encryption_in_image()
        {
            string message = string.Empty;
            do
            {
                Console.Write("Podaj nazwe wiadomosci do zaszyfrowania w obrazie. Enter - plik message.txt: ");
                string filename = Console.ReadLine();
                if (filename == string.Empty)
                {
                    filename = "message.txt";
                }
                message = Read_file_txt(filename);
                if (message == string.Empty)
                {
                    Console.WriteLine("Brak pliku lub plik pusty.");
                }
            } while (message == string.Empty);
            Console.WriteLine("Plik wczytany poprawnie. Dlugosc: " + message.Length);

            //zamiana string message czyli poszczegolnych char na int
            List<int> message_as_int_list = new List<int>();
            for (int i = 0; i < message.Length; i++)
            { message_as_int_list.Add((int)message[i]); }
            List<string> message_as_bits = new List<string>();

            int how_many_bits = Int_List_to_bit_string_List(message_as_int_list, ref message_as_bits);
            Console.WriteLine("Napis jako strumien bitow:");
            foreach (var x in message_as_bits)
            { Console.Write(x + " "); }

            //wczytujemy obraz w ktorym bedziemy szyfrowac wiadomosc
            Image<Bgr, Byte> img = null;
            do
            {
                Console.Write("\nPodaj nazwe obrazu. Enter - plik img.png: ");
                string filename = Console.ReadLine();
                if (filename == string.Empty)
                {
                    filename = "img.png";
                }
                try
                {
                    img = new Image<Bgr, byte>(filename);
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }
            } while (img == null);
            //Bgr color = img[5, 5];
            //Console.Write(format, "B ");
            //Console.Write(format, "G ");
            //Console.WriteLine(format, "R ");
            //Console.WriteLine(color.ToString());

            int[] rgb_bits = new int[3];
            int temp = 0;
            do
            {
                temp = 1;
                Console.Write("\n\nIle bitow dla RGB (wypisz oddzielajac spacjami): ");
                try
                {
                    var rgb_bits_as_string = Console.ReadLine().Split(' ');
                    for (int i = 0; i < rgb_bits.Length; i++)
                    {
                        int.TryParse(rgb_bits_as_string[i], out rgb_bits[i]);
                        if (0 <= rgb_bits[i] && rgb_bits[i] <= 8)
                        {
                            temp = temp & 1;
                        }
                        else
                        {
                            temp = 0;
                            Console.WriteLine("Blad na liczbie " + rgb_bits[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Błąd. " + e.Message);
                    temp = 0;
                }
            } while (temp == 0);

            Console.WriteLine("R\tG\tB");
            Console.WriteLine(rgb_bits[0] + "\t" + rgb_bits[1] + "\t" + rgb_bits[2]);
            Console.WriteLine("Size: " + img.Size);
            for (int i = 0, m = 0, n = 0; i < img.Size.Height && m < message.Length; i++)//m = znak message, n = bit znaku message (0-7)
            {
                for (int j = 0; j < img.Size.Width && m < message.Length; j++)
                {
                    Console.WriteLine("\n\nBGR[" + i + "," + j + "]: " + img[i, j].ToString());
                    int[] new_color = { Convert.ToInt32(img[i, j].Red), Convert.ToInt32(img[i, j].Green), Convert.ToInt32(img[i, j].Blue) };//zapisujemy stary kolor zeby go edytowac
                    for (int color_cnt = 0; color_cnt < 3 && m < message.Length; color_cnt++)
                    {
                        Console.WriteLine("Kolor " + color_cnt + " z RGB");
                        for (int cnt = rgb_bits[color_cnt] - 1; cnt >= 0 && m < message.Length; cnt--)
                        {
                            Console.WriteLine("Bit " + cnt);
                            int msg_piece;// = (message_as_bits[m][n]) == '0' ? 0 : 1;
                            msg_piece = 1 << cnt;//przesuniecie bitowe
                            int temp2 = 0;
                            var old_color_as_bit_string = int_to_bit_string(new_color[color_cnt], ref temp2);
                            var new_color_as_bit_string = string.Empty;
                            var msg_piece_as_bit_string = int_to_bit_string(msg_piece, ref temp2);
                            Console.WriteLine("Zapiszemy " + msg_piece + " czyli " + msg_piece_as_bit_string + " dla " + message_as_bits[m][n]);

                            if (message_as_bits[m][n] == '0')
                            {
                                for (int licz = 0; licz < msg_piece_as_bit_string.Length; licz++)
                                {
                                    if (msg_piece_as_bit_string[licz] == '1')
                                    {
                                        new_color_as_bit_string += '0';
                                    }
                                    else
                                    {
                                        new_color_as_bit_string += old_color_as_bit_string[licz];
                                    }
                                }
                            }
                            else if (message_as_bits[m][n] == '1')
                            {
                                for (int licz = 0; licz < msg_piece_as_bit_string.Length; licz++)
                                {
                                    if (msg_piece_as_bit_string[licz] == '1')
                                    {
                                        new_color_as_bit_string += '1';
                                    }
                                    else
                                    {
                                        new_color_as_bit_string += old_color_as_bit_string[licz];
                                    }
                                }
                            }
                            else
                            { Console.WriteLine("Nieznany blad. To nie powinno sie zdarzyc."); }

                            n++;
                            if (n >= 8)
                            {
                                m++;
                                n = 0;
                            }
                            Console.Write("Kolor " + new_color[color_cnt]);
                            new_color[color_cnt] = Bit_string_to_int32(new_color_as_bit_string);
                            Console.WriteLine(" Zamieniono na " + new_color[color_cnt]);
                        }
                    }
                    img[i, j] = new Bgr(Convert.ToDouble(new_color[2]), Convert.ToDouble(new_color[1]), Convert.ToDouble(new_color[0]));
                    Console.WriteLine("BGR[" + i + "," + j + "]: " + img[i, j].ToString());
                }
                //Console.WriteLine("m = " + m);
                //Console.WriteLine("i = " + i);
                if (m <= message.Length)
                { Console.WriteLine("Uwaga! Wiadomosc nie zmiescila sie w obrazie!"); }
            }
            img.Save("img2.png");
            Console.WriteLine("Obraz zapisany");
        }

        public static void Decryption_from_image()
        {
            //wczytujemy obraz z ktorego bedziemy odszyfrowywac wiadomosc
            Image<Bgr, Byte> img = null;
            do
            {
                Console.Write("\nPodaj nazwe obrazu. Enter - plik img2.png: ");
                string filename = Console.ReadLine();
                if (filename == string.Empty)
                {
                    filename = "img2.png";
                }
                try
                {
                    img = new Image<Bgr, byte>(filename);
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }
            } while (img == null);
            //Bgr color = img[5, 5];
            //Console.Write(format, "B ");
            //Console.Write(format, "G ");
            //Console.WriteLine(format, "R ");
            //Console.WriteLine(color.ToString());

            int[] rgb_bits = new int[3];
            int temp = 0;
            do
            {
                temp = 1;
                Console.Write("\n\nIle bitow dla RGB (wypisz oddzielajac spacjami): ");
                try
                {
                    var rgb_bits_as_string = Console.ReadLine().Split(' ');
                    for (int i = 0; i < rgb_bits.Length; i++)
                    {
                        int.TryParse(rgb_bits_as_string[i], out rgb_bits[i]);
                        if (0 <= rgb_bits[i] && rgb_bits[i] <= 8)
                        {
                            temp = temp & 1;
                        }
                        else
                        {
                            temp = 0;
                            Console.WriteLine("Blad na liczbie " + rgb_bits[i]);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Błąd. " + e.Message);
                    temp = 0;
                }
            } while (temp == 0);

            Console.WriteLine("R\tG\tB");
            Console.WriteLine(rgb_bits[0] + "\t" + rgb_bits[1] + "\t" + rgb_bits[2]);
            Console.WriteLine("Size: " + img.Size);
            string message = string.Empty;
            int msg_piece = 0;
            for (int i = 0, n = 7; i < img.Size.Height; i++)//n = bit znaku message (0-7)
            {
                for (int j = 0; j < img.Size.Width; j++)
                {
                    //Console.WriteLine("\n\nBGR[" + i + "," + j + "]: " + img[i, j].ToString());
                    int[] color = { Convert.ToInt32(img[i, j].Red), Convert.ToInt32(img[i, j].Green), Convert.ToInt32(img[i, j].Blue) };//zapisujemy stary kolor zeby go edytowac
                    for (int color_cnt = 0; color_cnt < 3; color_cnt++)//po kolei red green blue
                    {
                        //Console.WriteLine("Kolor " + color_cnt + " z RGB");
                        int temp2 = 0;
                        var color_as_bit_string = int_to_bit_string(color[color_cnt], ref temp2);
                        for (int cnt = 8 - rgb_bits[color_cnt]; cnt < 8; cnt++)
                        {
                            //Console.WriteLine("Bit " + (8 - cnt));

                            if (color_as_bit_string[cnt] == '1')
                            {
                                msg_piece = msg_piece | (1 << n);
                            }
                            n--;
                            if (n == -1)
                            {
                                message += (char)msg_piece;
                                //Console.WriteLine("Litera:\t" + (char)msg_piece);
                                msg_piece = 0;
                                n = 7;
                            }
                        }
                    }
                }
            }
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("decrypted.txt", false))
            {
                file.WriteLine(message);
                Console.WriteLine(message);
            }
            Console.WriteLine("Wiadomosc zapisana.");
        }
        public static string Read_file_txt(string filename)
        {
            try
            {
                StreamReader sr = new StreamReader(filename);
                string source = sr.ReadToEnd();
                return source;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }

        public static int Int_List_to_bit_string_List(List<int> tab_num, ref List<string> tab_bin)//return how many bits
        {
            int how_many_bits = 0;
            int bits = 0;
            tab_bin.Add(int_to_bit_string(tab_num[0], ref how_many_bits));
            for (int i = 1; i < tab_num.Count; i++)
            {
                tab_bin.Add(int_to_bit_string(tab_num[i], ref bits));
                how_many_bits += bits;
            }
            return how_many_bits;
        }
        public static string int_to_bit_string(int number, ref int how_many_bits)//changes int to 0-1 string
        {
            how_many_bits = 0;
            //Console.WriteLine("\nOryginal: " + number);
            string result = "";
            do
            {
                StringBuilder temp = new StringBuilder();
                for (var k = 7; k >= 0; k--)
                {
                    temp.Append((number & (1 << k)) == 0 ? '0' : '1');
                }
                result = temp + result;
                number = number >> 8;
                how_many_bits++;//po kazdej petli zamiast zwiekszac o 8, zwiekszamy o 1, a potem pomnozymy razy 8
            } while (number > 0);
            how_many_bits *= 8;//kazdy spelniony while to 8 bitow wyniku, wiec mnozymy razy 8
            //Console.WriteLine("\nWynik: " + result);
            return result;
        }
        public static int Bit_string_to_int32(string bit_string)
        {
            int number = 0;
            for (int i = 0; i < bit_string.Length; i++)
            {
                int temp = (bit_string[i] == '1' ? 1 : 0);
                temp = temp << (bit_string.Length - i - 1);
                number = number | temp;
            }
            return number;
        }
        public static Point Rand_point(int max_row, int max_col)//max values excluded
        {
            return new Point(rand.Next(0, max_row), rand.Next(0, max_col));
        }
        public static void Watermark()
        {
            Image<Hsv, Byte> img_hsv = null;
            do
            {
                Console.Write("\nPodaj nazwe obrazu. Enter - plik img.png: ");
                string filename = Console.ReadLine();
                if (filename == string.Empty)
                {
                    filename = "img.png";
                }
                try
                {
                    Image<Bgr, Byte> img_bgr = new Image<Bgr, byte>(filename);
                    img_hsv = img_bgr.Convert<Hsv, Byte>();
                }
                catch (Exception e)
                { Console.WriteLine(e.Message); }
            } while (img_hsv == null);

            if ((img_hsv.Width * img_hsv.Height) < 16)
            {
                Console.WriteLine("Obraz jest za maly do pracy ze znakiem wodnym.");
                return;
            }

            int seed = 0;
            do
            {
                Console.Write("\nPodaj ziarno generatora losowego: ");
                if (int.TryParse(Console.ReadLine(), out seed) && seed > 0)
                {
                    rand = new Random(seed);
                }
                else
                {
                    Console.WriteLine("Niepoprawna wartosc.");
                    seed = 0;
                }
            } while (seed == 0);

            double delta = 0;
            do
            {
                Console.Write("\nPodaj wartosc zmiany jasnosci pikseli. kodowanie > 0; dekodowanie < 0: ");
                if (double.TryParse(Console.ReadLine(), out delta) && delta > 0 && delta < 256)
                { }
                else if (delta < 0 && delta > -256)
                { }
                else
                {
                    Console.WriteLine("Niepoprawna wartosc.");
                    delta = 0;
                }
            } while (delta == 0);

            //obliczenie ilosci punktow mozliwych do uzycia
            int bad_points = 0;//liczba punktow niezdatnych do uzycia
            int good_points = 0;
            {
                for (int i = 0; i < img_hsv.Height; i++)
                {
                    for (int j = 0; j < img_hsv.Width; j++)
                    {
                        double new_value = img_hsv[i, j].Value - delta;
                        double new_value_2 = img_hsv[i, j].Value + delta;
                        
                        Console.Write("(" + i + ", " + j + ")\tOld: " + img_hsv[i, j].Value + "  ");
                        if (new_value < 0 || new_value > 255 || new_value_2 < 0 || new_value_2 > 255)
                        {
                            bad_points++;
                            Console.WriteLine("bad");
                        }
                        else
                        {
                            Console.WriteLine("good");
                        }
                    }
                }
                good_points = img_hsv.Width * img_hsv.Height - bad_points;
                Console.WriteLine("Jest " + bad_points + " zlych punktow.");
                Console.WriteLine("Jest " + good_points + " dobrych punktow.");
            }
            ////////////////////////////////

            int how_many_points = 0;
            do
            {
                int max_num_of_pairs = good_points/2;//limit 3/4 par calego obrazu
                Console.Write("\nPodaj liczbe par punktow do znaku wodnego Max = " + max_num_of_pairs + ": ");
                if (int.TryParse(Console.ReadLine(), out how_many_points) && how_many_points > 0 && how_many_points <= max_num_of_pairs)
                {
                    how_many_points *= 2;
                }
                else
                {
                    Console.WriteLine("Niepoprawna ilosc.");
                    how_many_points = 0;
                }
            } while (how_many_points == 0);

            int module = Convert.ToInt32(delta) * how_many_points;


            Console.WriteLine("Wymiary obrazu: " + img_hsv.Width + " x " + img_hsv.Height);
            Console.WriteLine(how_many_points + " zmienionych punktów");
            Console.WriteLine("Roznica jasnosci pikseli: " + delta);
            int sn = 0;
            List<Point> used_points = new List<Point>();
            for (int i = 0; i < how_many_points; i++)
            {
                List<Point> rejected_points = new List<Point>();//punkty odrzucone bo przekrecaja licznik
                Point p;
                bool simmilar = false;
                double new_value = 0;
                //pierwszy punkt
                do
                {
                    simmilar = false;
                    //p = Rand_point(img_hsv.Width, img_hsv.Height);
                    p = Rand_point(img_hsv.Height, img_hsv.Width);
                    foreach (Point x in used_points)
                    {
                        if (x.Equals(p))
                        {
                            simmilar = true;
                            break;
                        }
                    }
                    //Console.WriteLine("Wylosowalem " + p.ToString());
                    new_value = img_hsv[p.Y, p.X].Value - delta;
                    double new_value_2 = img_hsv[p.Y, p.X].Value + delta;
                    if (new_value < 0 || new_value > 255 || new_value_2 < 0 || new_value_2 > 255)
                    {
                        simmilar = true;
                        bool already_rejected = false;
                        foreach (Point x in rejected_points)
                        {
                            if (x.Equals(p))
                            {
                                already_rejected = true;
                                break;
                            }
                        }
                        if (already_rejected == true)
                        {
                            rejected_points.Add(p);
                        }
                    }
                    if (used_points.Count() + rejected_points.Count() == how_many_points)
                    {//to juz nie powinno sie zdarzyc
                        Console.WriteLine("Liczba uzytych punktow: " + used_points.Count());
                        Console.WriteLine("Liczba odrzuconych punktow: " + rejected_points.Count());
                        Console.WriteLine("Niemozliwe zakodowanie znaku wodnego z podanym ziarnem lub podana zmiana jasnosci.");
                        return;
                    }
                } while (simmilar == true);

                used_points.Add(p);
                Console.Write(i + ". ");
                //Console.Write("(" + p.X + ", " + p.Y + ")\tOld: " + img_hsv[p.X, p.Y].Value + "  ");
                Console.Write(string.Format(p.ToString() + " \tOld: {0}  ", img_hsv[p.Y, p.X].Value));
                img_hsv[p.Y, p.X] = new Hsv(img_hsv[p.Y, p.X].Hue, img_hsv[p.Y, p.X].Satuation, new_value);
                Console.WriteLine("\tNew: " + img_hsv[p.Y, p.X].Value);
                sn += Convert.ToInt32((delta > 0 ? new_value : new_value * -1));
                delta *= -1;//zmiana znaku, bo raz dodajemy a jazd odejmujemy te wartosc od jasnosci
            }
            if (delta > 0)
            {
                Console.Write("Sn = " + sn + " + " + module + " = ");
                sn += module;
                Console.WriteLine(sn);
            }
            else
            {//przez podanie delty ujemnej, sn liczone jest najpierw od odejmowania, wiec mnozymy razy -1
                sn *= -1;
                Console.Write("Sn = " + sn);
            }
            img_hsv.Save("img2.png");
            Console.WriteLine("Zapisano obraz img2.png");
        }
    }
}