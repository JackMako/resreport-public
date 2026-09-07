export interface Booking {
    Company: string;
    Given: string;
    Surname: string;
    Arrive_Date_Short: string;
    Nights: number;
    BkgSrc: string;
    No_Of_Visits: number;
    Note1: string;
    Notes: string;
    Status: string;
    DateMade_Short: string;
    Res_No: string;
    Group_Master: number;
    Cancelled_Date: Date;
}

export interface DraSection {
    title: string;
    groups: {
        company: string;
        lines: string[];
    }[];
}