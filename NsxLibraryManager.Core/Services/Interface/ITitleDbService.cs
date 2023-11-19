﻿using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Dto;

namespace NsxLibraryManager.Core.Services.Interface;

public interface ITitleDbService
{
    public Task ImportRegionAsync(string region);
    
    public Task ImportCnmtsAsync();
    
    public Task ImportVersionsAsync();
    
    public IEnumerable<string> GetRegionsToImport();

    public Task<uint> GetAvailableVersion(string titleTitleId);
    public IEnumerable<GameVersions> GetVersions(string titleTitleId);
    
    public Task<RegionTitle?> GetTitle(string titleTitleId);
    
    public Task<IEnumerable<Dlc>>  GetTitleDlc(string titleTitleId);
    
    public IEnumerable<PackagedContentMeta> GetTitleCnmts(string titleTitleId);
}