using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Services;

public class FtpStateService
{
    public event Action<FtpStatusUpdate> OnStatusUpdate;

    public event Action<FtpCompletedTransfer> OnTransferCompleted;
    
    public List<FtpStatusUpdate> CurrentTransfers { get; private set; } = [];
    public List<FtpCompletedTransfer> CompletedTransfers { get; private set; } = [];
    

    public void UpdateStatus(FtpStatusUpdate update)
    {
        var existingTransfer = CurrentTransfers.FirstOrDefault(t => t.TransferId == update.TransferId);
        if (existingTransfer is not null)
        {
            CurrentTransfers.Remove(existingTransfer);
        }
        CurrentTransfers.Add(update);
        OnStatusUpdate?.Invoke(update);
    }

    public void CompleteTransfer(FtpCompletedTransfer completed)
    {
        var existingTransfer = CurrentTransfers.FirstOrDefault(t => t.TransferId == completed.TransferId);
        if (existingTransfer is not null)
        {
            CurrentTransfers.Remove(existingTransfer);
        }
        
        CompletedTransfers.Insert(0, completed);
        if (CompletedTransfers.Count > 100)
        {
            CompletedTransfers.RemoveAt(CompletedTransfers.Count - 1);
        }

        OnTransferCompleted?.Invoke(completed);
    }
}