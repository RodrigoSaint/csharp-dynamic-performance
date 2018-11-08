using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TestePerformance
{
    class Client
    {
        public int Id { get; set; }
        public string Nome { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var total = 100000;
            ByObject(total);
            ByReflection(total);
            ByDynamic(total);
            ByDictionary(total);

            Console.ReadLine();
        }

        private static void ByReflection(int total)
        {
            var fields = new Dictionary<string, Type>();
            fields.Add("Id", typeof(int));
            fields.Add("Nome", typeof(string));
            var clientType = MyTypeBuilder.GetType("Client", fields);

            var listaCliente = new List<object>(total);

            for (int i = 0; i < total; i++)
            {
                var client = MyTypeBuilder.CreateNewObject(clientType);
                MyTypeBuilder.SetProperty(client, "Id", i);
                MyTypeBuilder.SetProperty(client, "Nome", $"Nome {i}");
                listaCliente.Add(client);
            }

            ShowMemoryUsage("reflection");
        }

        private static void CreateConstructor(TypeBuilder typeBuilder)
        {
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(
                                MethodAttributes.Public |
                                MethodAttributes.SpecialName |
                                MethodAttributes.RTSpecialName,
                                CallingConventions.Standard,
                                new Type[0]);
            //Define the reflection ConstructorInfor for System.Object
            ConstructorInfo conObj = typeof(object).GetConstructor(new Type[0]);

            //call constructor of base object
            ILGenerator il = constructor.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Call, conObj);
            il.Emit(OpCodes.Ret);
        }

        private static void ByDynamic(int total)
        {
            var listaClient = new List<dynamic>(total);
            for (int i = 0; i < total; i++)
            {
                dynamic client = new System.Dynamic.ExpandoObject();
                client.Id = i;
                client.Nome = $"Nome {i}";
                listaClient.Add(client);
            }
            ShowMemoryUsage("dynamic");
        }

        private static void ShowMemoryUsage(string title)
        {
            long memory = GC.GetTotalMemory(true);
            NumberFormatInfo nfi = new CultureInfo("pt-BR", false).NumberFormat;
            nfi.CurrencyGroupSeparator = ".";
            Console.WriteLine($"{title.PadRight(20, ' ')} {memory.ToString().PadLeft(20, ' ')}");
            GC.Collect();
        }

        private static void ByDictionary(int total)
        {
            var listaClient = new List<IDictionary<string, object>>(total);

            for (int i = 0; i < total; i++)
            {
                var client = new Dictionary<string, object>();
                client.Add("Id", i);
                client.Add("Nome", $"Nome {i}");
                listaClient.Add(client);
            }
            ShowMemoryUsage("dictionary");
        }

        private static void ByObject(int total)
        {
            var listaClient = new List<Client>(total);

            for (int i = 0; i < total; i++)
                listaClient.Add(new Client() { Id = i, Nome = $"Nome {i}" });
            ShowMemoryUsage("Object");

        }
    }
}
