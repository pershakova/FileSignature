namespace FileSignature
{
   internal class Chunk
    {
        public int Number { get; set; }
        public byte[] Bytes { get; set; }
        public string Hash { get; set; }
    }
}
