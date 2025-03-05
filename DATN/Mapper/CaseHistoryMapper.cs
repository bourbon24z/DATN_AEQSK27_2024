namespace DATN.Mapper;

using DATN.Dto;
using DATN.Models;

public static class CaseHistoryMapper
{
	public static CaseHistoryDto ToCaseHistoryDto(this CaseHistory caseHistory)
	{
		return new CaseHistoryDto
		{
			ProgressNotes = caseHistory.ProgressNotes,
			Time = caseHistory.Time,
			StatusOfMr = caseHistory.StatusOfMr
		};
	}
}
	
