using ERD2DB.Main;

namespace ERD2DB
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // STA 모드에서는 비동기 호출을 피해야 함
            Main36MA.StartApplication();
        }
    }
}