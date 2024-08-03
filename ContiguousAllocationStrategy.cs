
// Implement contiguous block allocation strategy
class ContiguousAllocationStrategy : IAllocationStrategy
{
    public int Allocate(bool[] allocationTable, int numberOfBlocks)
    {
        int contiguousCount = 0;
        for (int i = 0; i < allocationTable.Length; i++)
        {
            if (!allocationTable[i])
            {
                contiguousCount++;
                if (contiguousCount == numberOfBlocks)
                {
                    return i - numberOfBlocks + 1;
                }
            }
            else
            {
                contiguousCount = 0;
            }
        }
        return -1;
    }
}
