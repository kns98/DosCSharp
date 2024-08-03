
// Define a strategy interface for block allocation
interface IAllocationStrategy
{
    int Allocate(bool[] allocationTable, int numberOfBlocks);
}
