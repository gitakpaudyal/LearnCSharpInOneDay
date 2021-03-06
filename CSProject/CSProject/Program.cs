﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static System.Console;
using static System.Convert;

namespace CSProject
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Staff> myStaff;
            FileReader fr = new FileReader();
            int month = 0;
            int year = 0;

            //Get the year to process
            while (year == 0)
            {
                Write("\nPlease enter the year: ");

                try
                {
                    year = ToInt32(ReadLine());

                    if (year < 2000 || year > 2100)
                    {
                        year = 0;
                        WriteLine("Year is out of range.");
                    }
                }
                catch (FormatException e)
                {
                    WriteLine("Error entering year: {0}", e.Message);
                }
            }

            //Get the month to process
            while (month == 0)
            {
                Write("\nPlease enter the month: ");

                try
                {
                    month = ToInt32(ReadLine());

                    if (month < 1 || month > 12)
                    {
                        month = 0;
                        WriteLine("Please enter a value between 1 and 12.");
                    }
                }
                catch (FormatException e)
                {
                    WriteLine("Error entering month: {0}", e.Message);
                }
            }

            myStaff = fr.ReadFile();
            for (int i = 0; i < myStaff.Count; i++)
            {
                try
                {
                    //Retrieve the hours worked for the current staff
                    Write("\nEnter hours worked for {0}: ", myStaff[i].NameOfStaff);
                    myStaff[i].HoursWorked = ToInt32(ReadLine());

                    //Calculate the pay and write it to the screen
                    myStaff[i].CalculatePay();
                    WriteLine(myStaff[i].ToString());
                }
                catch (Exception e)
                {
                    WriteLine("Error entering hours: {0}", e.Message);
                    //Decrement the counter so we process the same staff again
                    i--;
                }
            }

            //Create a pay slip object to output the pay slips and summary
            PaySlip ps = new PaySlip(month, year);
            ps.GeneratePaySlip(myStaff);
            ps.GenerateSummary(myStaff);

            WriteLine("Payroll complete. Press any key to continue.");
            ReadKey();
        }
    }

    class Staff
    {
        //Fields
        private float hourlyRate;
        private int hWorked;

        //Properties
        //This class and its subclasses can set TotalPay
        public float TotalPay { get; protected set; }
        //Only this class can set BasicPay and NameOfStaff
        public float BasicPay { get; private set; }
        public string NameOfStaff { get; private set; }

        public int HoursWorked
        {
            get
            {
                return hWorked;
            }
            set
            {
                if (value > 0)
                    hWorked = value;
                else
                    hWorked = 0;
            }
        }

        //Constructor
        public Staff(string name, float rate)
        {
            NameOfStaff = name;
            hourlyRate = rate;
        }

        //Methods
        public virtual void CalculatePay()
        {
            WriteLine("Calculating Pay...");

            BasicPay = hWorked * hourlyRate;
            TotalPay = BasicPay;
        }

        public override string ToString()
        {
            return "\nName of staff: " + NameOfStaff
                + "\nHourly rate = " + hourlyRate
                + ", Hours worked = " + hWorked
                + "\nBasic pay = " + BasicPay 
                + "\nTotal pay = " + TotalPay;
        }
    }

    class Manager : Staff
    {
        //Fields
        private const float managerHourlyRate = 50;

        //Properties
        //Only this class can set the Allowance
        public int Allowance { get; private set; }

        //Constructor
        public Manager(string name)
            : base(name, managerHourlyRate)
        {
            //Nothing to do here
        }

        //Methods
        public override void CalculatePay()
        {
            base.CalculatePay();

            Allowance = 1000;
            if (HoursWorked > 160)
                TotalPay = BasicPay + Allowance;
        }

        public override string ToString()
        {
            return base.ToString()
                + "\nAllowance = " + Allowance;
        }
    }

    class Admin : Staff
    {
        //Fields
        private const float overtimeRate = 15.5f;
        private const float adminHourlyRate = 30f;

        //Properties
        //Only this class can set Overtime
        public float Overtime { get; private set; }

        //Constructor
        public Admin(string name)
            : base(name, adminHourlyRate)
        {
            //Nothing to do here
        }

        //Methods
        public override void CalculatePay()
        {
            base.CalculatePay();

            if (HoursWorked > 160)
            {
                Overtime = overtimeRate * (HoursWorked - 160);
                TotalPay = BasicPay + Overtime;
            }
        }

        public override string ToString()
        {
            return base.ToString()
                + "\nOvertime = " + Overtime;
        }
    }

    class FileReader
    {
        public List<Staff> ReadFile()
        {
            List<Staff> myStaff = new List<Staff>();
            string[] result = new string[2];
            string path = "staff.txt";
            string[] separator = { ", " };

            if (File.Exists(path))
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    while (sr.EndOfStream == false)
                    {
                        //Read a line from the file, storing the name in the first array element,
                        //and the position in the second element.
                        result = sr.ReadLine().Split(separator, StringSplitOptions.RemoveEmptyEntries);

                        //Check the position, and add the appropriate Staff object to the list.
                        if (result[1] == "Manager")
                            myStaff.Add(new Manager(result[0]));
                        else if (result[1] == "Admin")
                            myStaff.Add(new Admin(result[0]));
                        else
                            WriteLine("Invalid position: {0}", result[1]);
                    }

                    sr.Close();
                }
            }
            else
                WriteLine("File not found: {0}", path);

            return myStaff;
        }
    }

    class PaySlip
    {
        //Fields
        private int month,
            year;

        //Enum (private by default inside a class)
        enum MonthsOfYear { JAN = 1, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC }

        //Constructor
        public PaySlip(int payMonth, int payYear)
        {
            month = payMonth;
            year = payYear;
        }

        //Methods
        public void GeneratePaySlip(List<Staff> myStaff)
        {
            string path;

            WriteLine("Generating pay slips...");

            foreach (Staff f in myStaff)
            {
                path = f.NameOfStaff + ".txt";

                using (StreamWriter sw = new StreamWriter(path, true))
                {
                    //Print the element # "month" from the MonthsOfYear enum
                    sw.WriteLine("PAY SLIP FOR {0} {1}", (MonthsOfYear) month, year);

                    //Generate a string by repeating the '=' char 25 times.
                    sw.WriteLine(new String('=', 25));
                    sw.WriteLine("Name of Staff: {0}", f.NameOfStaff);
                    sw.WriteLine("Hours Worked: {0}", f.HoursWorked);
                    sw.WriteLine("");

                    //Use [C]urrency format for the BasicPay amount.
                    sw.WriteLine("Basic Pay: {0:C}", f.BasicPay);

                    //Depending on the typeof Staff class being processed, cast f into an
                    //object of that type, so we can access the properties of that class.
                    if (f.GetType() == typeof(Manager))
                        sw.WriteLine("Allowance: {0:C}", ((Manager)f).Allowance);
                    else if (f.GetType() == typeof(Admin))
                        sw.WriteLine("Overtime Pay: {0:C}", ((Admin)f).Overtime);

                    sw.WriteLine("");
                    sw.WriteLine(new String('=', 25));
                    sw.WriteLine("Total Pay: {0:C}", f.TotalPay);
                    sw.WriteLine(new String('=', 25));

                    sw.Close();
                }
            }
        }

        public void GenerateSummary(List<Staff> myStaff)
        {
            string path = "summary.txt";

            WriteLine("Generating summary...");

            //Retrieve a list of staff with less than 10 hours.
            var result =
                from st in myStaff
                where st.HoursWorked < 10
                orderby st.NameOfStaff
                select new { st.NameOfStaff, st.HoursWorked };

            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine("Staff with less than 10 working hours for {0} {1}", (MonthsOfYear) month, year);
                sw.WriteLine("");

                foreach (var f in result)
                {
                    sw.WriteLine("Name of Staff: {0}, Hours Worked: {1}", f.NameOfStaff, f.HoursWorked);
                }
                sw.WriteLine(new String('=', 25));

                sw.Close();
            }
        }

        public override string ToString()
        {
            return "\nMonth = " + (MonthsOfYear) month
                + "\nYear = " + year;
        }
    }
}
