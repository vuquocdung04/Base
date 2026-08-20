using System;

public sealed class PurchaseTicket
{
    public static readonly PurchaseTicket Failed = new(false, null);

    private Action _commit;

    private PurchaseTicket(bool success, Action commit)
    {
        Success = success;
        _commit = commit;
    }

    public bool Success { get; }

    public static PurchaseTicket Ok(Action commit = null) => new(true, commit);

    public void Commit()
    {
        Action commit = _commit;
        _commit = null;

        commit?.Invoke();
    }
}
