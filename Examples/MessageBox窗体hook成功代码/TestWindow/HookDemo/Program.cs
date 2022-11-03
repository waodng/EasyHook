using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotNetDetour;

namespace HookDemo
{
    public class CustomMonitor : IMethodMonitor //自定义一个类并继承IMethodMonitor接口

    {
        [Monitor("ZBHR_TXT", "MainForm")] //你要hook的目标方法的名称空间，类名

        public string MonitorCaiji() //方法签名要与目标方法一致

        {
            Console.WriteLine("已经开始注入数据采集程序");
            return "B" + Ori();
        }


        [Monitor("HookDemo", "Student")] //你要hook的目标方法的名称空间，类名

        public string Get() //方法签名要与目标方法一致

        {
            return "B" + Ori();
        }

        [Monitor("HookDemo", "Student")]
        public string GetA(string str)
        {
            return str+"B" + Ori();
        }


        [MethodImpl(MethodImplOptions.NoInlining)]

        [Original] //原函数标记

        public string Ori() //方法签名要与目标方法一致
        {
            return null; //这里写什么无所谓，能编译过即可
        }
    }


    public class Student
    {
        public Student()
        {

        }

        public string Get()
        {
            return "A";
        }
        
        public string GetA(string str)
        {
            return str;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Student student = new Student();
            Console.WriteLine(student.Get());
            Console.WriteLine(student.GetA("hello"));
            Monitor.Install();
            Console.WriteLine(student.Get());
            Console.WriteLine(student.GetA("hello"));

            Console.ReadKey();
        }
    }
}
