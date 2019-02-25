namespace PipeVisionConsole.Config
{
    class PvcConfig
    {
        public string DbConnection { get; set; }
        public string GoServerBaseAddress { get; set; }
        public string GoServerUser { get; set; }
        public string GoServerPass { get; set; }
        public int DbCommandTimeout { get; set; }
    }
}
