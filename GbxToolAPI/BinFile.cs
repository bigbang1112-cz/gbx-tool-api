namespace GbxToolAPI;

public class BinFile
{
    public byte[] Data { get; }

    public BinFile(byte[] data)
    {
        Data = data;
    }
    
    public static explicit operator byte[](BinFile binFile)
    {
        return binFile.Data;
    }
}
