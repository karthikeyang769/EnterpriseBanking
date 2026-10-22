using System;
using System.Collections.Generic;

namespace SharedContracts.Responses;

public class PagedResponse<T> : ApiResponse<List<T>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }

    public PagedResponse()
    {
        Data = new List<T>();
    }

    public PagedResponse(List<T> data, int pageNumber, int pageSize, int totalRecords, string message = "Success")
    {
        Success = true;
        Data = data;
        Message = message;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}
