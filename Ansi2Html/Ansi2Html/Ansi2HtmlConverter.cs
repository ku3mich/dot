using System;

using System.Text;

namespace Ansi2Html
{
    internal class selem
    {
        public int[] digit = new int[8];
        public int digitcount;
        public selem next;
    }

    public class Ansi2HtmlConverter : IAnsi2HtmlConverter
    {
        private const int EOF = -1;
        private const int ESC = 27;

        private int filepos;
        private int fgetc(string s)
        {
            return filepos < s.Length ? s[filepos++] : EOF;
        }
        private char getNextChar(string fp)
        {
            int c;

            if ((c = fgetc(fp)) != EOF)
                return (char)c;
            throw new Exception("Unable parse input");
        }

        private StringBuilder sb;
        private void printf(string format, params object[] args)
        {
            sb.AppendFormat(format, args);
        }
        private void printf(char c)
        {
            sb.Append(c);
        }

        private selem parseInsert(char[] s)
        {
            selem firstelem = null;
            selem momelem = null;
            int[] digit = new int[8];
            int digitcount = 0;
            //int pos = 0;
            for (int pos = 0; pos < 1024; pos++)
            {
                if (s[pos] == '[')
                    continue;
                if (s[pos] == ';' || s[pos] == 0)
                {
                    if (digitcount <= 0)
                    {
                        digit[0] = '\0';
                        digitcount = 1;
                    }

                    selem newelem = new selem();
                    for (int a = 0; a < 8; a++)
                        newelem.digit[a] = digit[a];
                    newelem.digitcount = digitcount;
                    newelem.next = null;
                    if (momelem == null)
                        firstelem = newelem;
                    else
                        momelem.next = newelem;
                    momelem = newelem;
                    digitcount = 0;

                    if (s[pos] == 0)
                        break;
                }
                else if (digitcount < 8)
                {
                    digit[digitcount] = s[pos] - '0';
                    digitcount++;
                }
            }
            return firstelem;
        }

        public bool htop_fix { get; set; }
        public bool word_wrap { get; set; }

        public string Convert(string i)
        {
            sb = new StringBuilder();
            sb.AppendLine("<pre>");
            string fp = i;

            bool line_break = false;

            int c;
            int fc = -1; //Standard Foreground Color //IRC-Color+8
            int bc = -1; //Standard Background Color //IRC-Color+8
            bool ul = false; //Not underlined
            bool bo = false; //Not bold
            bool bl = false; //No Blinking

            int line = 0;
            //int momline = 0;
            int newline = -1;
            filepos = 0;

            while ((c = fgetc(fp)) != EOF)
            {
                if ((c == ESC))
                {
                    //Saving old values
                    int ofc = fc;
                    int obc = bc;
                    bool oul = ul;
                    bool obo = bo;
                    bool obl = bl;
                    //Searching the end (a letter) and safe the insert:
                    c = '0';
                    char[] buffer = new char[1024];
                    int counter = 0;
                    while ((c < 'A') || ((c > 'Z') && (c < 'a')) || (c > 'z'))
                    {
                        c = getNextChar(fp);
                        buffer[counter] = (char)c;
                        if (c == '>') //end of htop
                            break;

                        counter++;
                        if (counter > 1022)
                            break;
                    }
                    buffer[counter - 1] = (char)0;
                    selem elem;
                    switch (c)
                    {
                        case 'm':
                            //printf("\n%s\n",buffer); //DEBUG
                            elem = parseInsert(buffer);
                            selem momelem = elem;
                            while (momelem != null)
                            {
                                //jump over zeros
                                int mompos = 0;
                                while (mompos < momelem.digitcount && momelem.digit[mompos] == 0)
                                    mompos++;
                                if (mompos == momelem.digitcount) //only zeros => delete all
                                {
                                    bo = false;
                                    ul = false;
                                    bl = false;
                                    fc = -1;
                                    bc = -1;
                                }
                                else
                                {
                                    int temp;
                                    switch (momelem.digit[mompos])
                                    {
                                        case 1:
                                            bo = true;
                                            break;
                                        case 2:
                                            if (mompos + 1 < momelem.digitcount)
                                                switch (momelem.digit[mompos + 1])
                                                {
                                                    case 1: //Reset blink and bold
                                                        bo = false;
                                                        bl = false;
                                                        break;
                                                    case 4: //Reset underline
                                                        ul = false;
                                                        break;
                                                    case 7: //Reset Inverted
                                                        temp = bc;

                                                        if (fc == -1 || fc == 9)

                                                            bc = 0;

                                                        else
                                                            bc = fc;

                                                        if (temp == -1 || temp == 9)

                                                            fc = 7;

                                                        else
                                                            fc = temp;

                                                        break;
                                                }
                                            break;
                                        case 3:
                                            if (mompos + 1 < momelem.digitcount)
                                                fc = momelem.digit[mompos + 1];
                                            break;
                                        case 4:
                                            if (mompos + 1 == momelem.digitcount)
                                                ul = true;
                                            else
                                                bc = momelem.digit[mompos + 1];
                                            break;
                                        case 5:
                                            bl = true;
                                            break;
                                        case 7: // TODO: Inverse
                                            temp = bc;
                                            if (fc == -1 || fc == 9)
                                                bc = 0;
                                            else
                                                bc = fc;
                                            if (temp == -1 || temp == 9)
                                                fc = 7;
                                            else
                                                fc = temp;
                                            break;
                                    }
                                }
                                momelem = momelem.next;
                            }
                            break;
                        case 'H':
                            if (htop_fix) //a lil dirty ...
                            {
                                elem = parseInsert(buffer);
                                selem second = elem.next ?? elem;
                                newline = second.digit[0] - 1;
                                if (second.digitcount > 1)
                                    newline = (newline + 1) * 10 + second.digit[1] - 1;
                                if (second.digitcount > 2)
                                    newline = (newline + 1) * 10 + second.digit[2] - 1;

                                if (newline < line)
                                    line_break = true;
                            }
                            break;
                    }
                    if (htop_fix)
                        if (line_break)
                        {
                            for (; line < 80; line++)
                                printf(" ");
                        }
                    //Checking the differeces
                    if ((fc != ofc) || (bc != obc) || (ul != oul) || (bo != obo) || (bl != obl)) //ANY Change
                    {
                        if ((ofc != -1) || (obc != -1) || (!oul) || (!obo) || (!obl))
                            printf("</span>");
                        if ((fc != -1) || (bc != -1) || (!ul) || (!bo) || (!bl))
                        {
                            printf("<span style=\"");
                            switch (fc)
                            {
                                case 0:
                                    printf("color:black;");
                                    break; //Black
                                case 1:
                                    printf("color:red;");
                                    break; //Red
                                case 2:
                                    printf("color:green;");

                                    break; //Green
                                case 3:
                                    printf("color:olive;");

                                    break; //Yellow
                                case 4:
                                    printf("color:blue;");

                                    break; //Blue
                                case 5:
                                    printf("color:purple;");
                                    break; //Purple
                                case 6:
                                    printf("color:teal;");
                                    break; //Cyan
                                case 7:
                                    printf("color:gray;");
                                    break; //White
                                case 9:
                                    printf("color:black;");
                                    break; //Reset
                            }
                            switch (bc)
                            {
                                //case -1: printf("background-color:white; "); break; //StandardColor
                                case 0:
                                    printf("background-color:black;");
                                    break; //Black
                                case 1:
                                    printf("background-color:red;");
                                    break; //Red
                                case 2:
                                    printf("background-color:green;");
                                    break; //Green
                                case 3:
                                    printf("background-color:olive;");
                                    break; //Yellow
                                case 4:
                                    printf("background-color:blue;");
                                    break; //Blue
                                case 5:
                                    printf("background-color:purple;");
                                    break; //Purple
                                case 6:
                                    printf("background-color:teal;");
                                    break; //Cyan
                                case 7:
                                    printf("background-color:gray;");
                                    break; //White
                                case 9:
                                    printf("background-color:white;");
                                    break; //Reset
                            }
                            if (ul)
                                printf("text-decoration:underline;");
                            if (bo)
                                printf("font-weight:bold;");
                            if (bl)
                                printf("text-decoration:blink;");

                            printf("\">");
                        }
                    }
                }
                else if (c == 13 && htop_fix)
                {
                    for (; line < 80; line++)
                        printf(" ");
                    line = 0;
                    //momline++;
                    printf('\n');
                }
                else if (c != 8)
                {
                    line++;
                    if (line_break)
                    {
                        printf('\n');
                        line = 0;
                        line_break = false;
                        //momline++;
                    }
                    if (newline >= 0)
                    {
                        while (newline > line)
                        {
                            printf(' ');
                            line++;
                        }
                        newline = -1;
                    }
                    switch (c)
                    {
                        case '&':
                            printf("&amp;");
                            break;
                        case '\"':
                            printf("&quot;");
                            break;
                        case '<':
                            printf("&lt;");
                            break;
                        case '>':
                            printf("&gt;");
                            break;
                        case '\n':
                            //momline++;
                            line = 0;
                            printf((char)c);
                            break;
                        case '\r':
                            break;
                        default:
                            printf((char)c);
                            break;
                    }
                }
            }
            sb.Append("</pre>");
            return sb.ToString();
        }
    }
}