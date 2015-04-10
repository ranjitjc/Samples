using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace GarbageCollectionDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            A a = new A();
            C b = new C();
            GC.Collect();
            a = null;
            GC.Collect();
            b.Dispose();
            //Following code will not work bcs b is already disposed.
            //b.DoSomeWork();
            GC.Collect();
            Console.WriteLine("\n\n");
            

            using (C b1 = new C())
            {
                b1.DoSomeWork();
            }

            Console.ReadLine();
        }
    }


    public class A {
        private string className = "A";
        public A()
        {
            Console.WriteLine("Class A: Constructor from " + className);
        }
        public A(string className)
        {
            this.className = className;
            Console.WriteLine("Class A: Constructor from " + this.className);
        }

       ~A()
        {
            Console.WriteLine("Class A: Destructor from " + className);
        }
    }

    public class X
    {
        public string className = "X";
        public X()
        {
            Console.WriteLine("Class X: Constructor from " + className);
        }
        public X(string className)
        {
            this.className = className;
            Console.WriteLine("Class X: Constructor from " + this.className);
        }

        ~X()
        {
            Console.WriteLine("Class X: Destructor from " + className);
        }
    }

    public class B : IDisposable
    {
        private string className = "B";
        private bool disposed = false;
        private A managedResource = new A("B");
        public B()
        {
            Console.WriteLine("Class B: Constructor from " + className);
        }
        public B(string className)
        {
            this.className = className;
            Console.WriteLine("Class B: Constructor from " + this.className);
        }

        public virtual void DoSomeWork()
        {
            if (disposed) throw new ObjectDisposedException("This Object is already disposed.");
            Console.WriteLine("Class B: DoSomeWork()");

        }

        //If your programming language does not support a construct like the using statement in C# or Visual Basic, 
        //or if you prefer not to use it, you can call the IDisposable.Dispose implementation from the finally block of a try/catch statement. 
        //The following example replaces the using block in the previous example with a try/catch/finally block. 
        public int WordCount(string filename)
        {
            if (disposed) throw new ObjectDisposedException("This Object is already disposed.");
            Console.WriteLine("Class B: WordCount(int)");
            int nWords = 0;
            String pattern = @"\b\w+\b";
            if (!File.Exists(filename))
                throw new FileNotFoundException("The file does not exist.");

            string txt = String.Empty;
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(filename);
                txt = sr.ReadToEnd();
            }
            finally
            {
                if (sr != null) sr.Dispose();
            }
            nWords = Regex.Matches(txt, pattern).Count;
            return nWords;
        }

        ~B()
        {
            Console.WriteLine("Destructor of Class " + className);
            Dispose(false);
        }

        public void Dispose()
        {
            Console.WriteLine("Class B: Dispose() from " + className);
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {

                // free managed resources
                if (managedResource != null)
                {
                    Console.WriteLine("Class B: Dispose(bool) : Managed Resource Cleanup.");
                    //managedResource.Dispose();
                    managedResource = null;
                }

            }

            disposed = true;


        }

    }


    public class C : B, IDisposable    
    {
        // Flag: Track whether Dispose has been called. 
        private bool disposed = false;
        //Unmanaged Resource , 
        // Pointer to an external unmanaged resource. 
        private IntPtr nativeResource = Marshal.AllocHGlobal(100);
        //Managed Resource
        private X managedResource = new X("C");

        public C(): base("C")
        {
            Console.WriteLine("Class C: Constructor");
        }


        public override void DoSomeWork()
        {
            if (disposed) throw new ObjectDisposedException("This Object is already disposed.");
            Console.WriteLine("Class C: DoSomeWork()");
            managedResource.className = "XX";
            base.DoSomeWork();

        }

        


        // NOTE:
        // Use C# destructor syntax for finalization code. 
        // This destructor will run only if the Dispose method 
        // does not get called. 
        // It gives your base class the opportunity to finalize. 
        // Do not provide destructors in types derived from this class.
        ~C()
        {
            Console.WriteLine("Class C: Destructor");
            // Do not re-create Dispose clean-up code here. 
            // Calling Dispose(false) is optimal in terms of 
            // readability and maintainability.
            Dispose(false);
        }

        // Public implementation of Dispose pattern callable by consumers. 
        // Implement IDisposable. 
        // Do not make this method virtual. 
        // A derived class should not be able to override this method. 
        public new void Dispose()
        {
            Console.WriteLine("Class C: Dispose()");
             Dispose(true);
             // This object will be cleaned up by the Dispose method. 
             // Therefore, you should call GC.SupressFinalize to 
             // take this object off the finalization queue 
             // and prevent finalization code for this object 
             // from executing a second time.
            GC.SuppressFinalize(this);
        }

       // The bulk of the clean-up code is implemented in Dispose(bool)
        // Protected implementation of Dispose pattern. 
        // Dispose(bool disposing) executes in two distinct scenarios. 
        // If disposing equals true, the method has been called directly 
        // or indirectly by a user's code. Managed and unmanaged resources 
        // can be disposed. 
        // If disposing equals false, the method has been called by the 
        // runtime from inside the finalizer and you should not reference 
        // other objects. Only unmanaged resources can be disposed. 
        protected override void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called. 
            if (disposed) return;

            // If disposing equals true, dispose all managed 
            // and unmanaged resources. 
            if (disposing)
            {

                // free managed resources
                if (managedResource != null)
                {
                    Console.WriteLine("Class C: Dispose(bool) : Managed Resource Cleanup.");
                    //managedResource.Dispose();
                    managedResource = null;
                }

            }
            
            
            // free native resources if there are any.
            if (nativeResource != IntPtr.Zero) 
            {
                Console.WriteLine("Class C: Dispose(bool) :  Unmanaged Resource Cleanup.");
                Marshal.FreeHGlobal(nativeResource);
                nativeResource = IntPtr.Zero;
            }
            disposed = true;

            // If it is available, make the call to the
            // base class's Dispose(Boolean) method
            base.Dispose(disposing);

            
        }
    }
}
