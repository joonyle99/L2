public abstract class ItemInstance { }
public abstract class ItemInstance<TData> : ItemInstance where TData : ItemData
{
    protected ItemInstance(TData data)
    {
        Data = data;
    }

    public TData Data { get; private set; }
}
