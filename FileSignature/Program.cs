using System;
using System.IO;

namespace FileSignature
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private int Run()
        {
            var fileExists = false;
            var chunkWasEntered = false;
            var filePath = string.Empty;
            int fileChunk = 1;
            long fileSize = 0;

            try
            {
                while (!fileExists)
                {
                    Console.WriteLine("Enter a file path:");

                    filePath = Console.ReadLine();
                    var file = new FileInfo(filePath);

                    fileExists = file.Exists;

                    if (fileExists)
                    {
                        fileSize = file.Length;
                    }

                    Console.WriteLine(fileExists ? $"File {file.FullName} exists and has size {fileSize} bite" : $"File {file.FullName} does not exist");
                }

                while (!chunkWasEntered)
                {
                    Console.WriteLine("Enter file chunk (positive digits), Mb:");
                    var parseResult = int.TryParse(Console.ReadLine(), out fileChunk);
                    if (!parseResult)
                    {
                        chunkWasEntered = false;
                        Console.WriteLine("Chunk was not parsed");
                    }
                    else
                    {
                        chunkWasEntered = fileChunk > 0;
                        if (fileChunk > fileSize)
                        {
                            chunkWasEntered = false;
                            Console.WriteLine($"Chunk can not be bigger than the file size {fileSize}");
                        }
                    }

                    Console.WriteLine(chunkWasEntered ? $"Chunk was entered: {fileChunk}" : $"Chunk was not entered or less than the 0: {fileChunk}");
                }

                new FileReader(filePath, fileChunk).Process();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}
