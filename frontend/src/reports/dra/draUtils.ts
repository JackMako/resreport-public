import Papa from "papaparse";
import { format, isSameDay, isToday, parse } from "date-fns";
import type { Booking, DraSection } from "./types.ts";

export function parseArrivalCSV(file: File): Promise<Booking[]> {
    return new Promise((resolve, reject) => {
        Papa.parse(file, {
            header: true,
            skipEmptyLines: true,
            complete: (results) => {
                const bookings = results.data.map((row: any): Booking => ({
                    Company: row.Company ?? "",
                    Given: row.Given ?? "",
                    Surname: row.Surname ?? "",
                    Arrive_Date_Short: row.Arrive_Date_Short ?? "",
                    Nights: Number(row.Nights ?? 0),
                    BkgSrc: row["Bkg Source"] ?? row.BkgSrc ?? "",
                    No_Of_Visits: Number(row.No_Of_Visits ?? 0),
                    Note1: row.Note1 ?? "",
                    Notes: row.Notes ?? "",
                    Status: row.Status ?? "",
                    DateMade_Short: row.DateMade_Short ?? "",
                    Res_No: row.Res_No ?? "",
                    Group_Master: Number(row.Group_Master ?? 0),
                    Cancelled_Date: row.Cancelled_Date
                        ? parse(row.Cancelled_Date, "d/M/yyyy", new Date())
                        : new Date(0),
                }));

                resolve(filterBookings(bookings));
            },
            error: reject,
        });
    });
}

function filterBookings(bookings: Booking[]): Booking[] {
    const seenResNos = new Set<string>();

    return bookings.filter((booking) => {
        if (seenResNos.has(booking.Res_No)) return false;
        seenResNos.add(booking.Res_No);
        return true;
    });
}

function cleanName(name: string): string {
    const prefixes = ["mr", "mrs", "ms", "miss", "dr", "prof"];

    return name
        .split(" ")
        .filter((word) => !prefixes.includes(word.toLowerCase()))
        .map((word) => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
        .join(" ")
        .trim();
}

function parseDate(dateStr: string) {
    return parse(dateStr, "d/M/yyyy", new Date());
}

function formatBookingDate(dateStr: string): string {
    const date = parseDate(dateStr);
    const currentYear = new Date().getFullYear();

    return date.getFullYear() === currentYear
        ? format(date, "dd/MM")
        : format(date, "d/MM/yyyy");
}

export function generateDraSections(
    bookings: Booking[],
    useResidentNotes: boolean
): DraSection[] {
    const today = new Date();

    const sorted = [...bookings].sort(
        (a, b) =>
            parseDate(a.Arrive_Date_Short).getTime() -
            parseDate(b.Arrive_Date_Short).getTime()
    );

    const sections = [
        {
            title: "Long term corporate bookings entered:",
            bookings: sorted.filter(
                (b) =>
                    isSameDay(parseDate(b.DateMade_Short), today) &&
                    b.Nights >= 7 &&
                    b.Status !== "Cancelled"
            ),
            hideArriveDate: false,
            hideCompany: false,
        },
        {
            title: "Corporate Bookings Entered:",
            bookings: sorted.filter(
                (b) =>
                    isSameDay(parseDate(b.DateMade_Short), today) &&
                    b.Company !== "Leisure" &&
                    b.Status !== "Cancelled"
            ),
            hideArriveDate: false,
            hideCompany: false,
        },
        {
            title: "Leisure Bookings entered:",
            bookings: sorted.filter(
                (b) =>
                    isSameDay(parseDate(b.DateMade_Short), today) &&
                    b.Company === "Leisure" &&
                    b.Status !== "Cancelled"
            ),
            hideArriveDate: false,
            hideCompany: true,
        },
        {
            title: "Arrivals:",
            bookings: sorted.filter(
                (b) =>
                    isSameDay(parseDate(b.Arrive_Date_Short), today) &&
                    b.Status !== "Cancelled"
            ),
            hideArriveDate: true,
            hideCompany: false,
        },
        {
            title: "Cancellations:",
            bookings: sorted.filter(
                (b) => b.Status === "Cancelled" && isToday(b.Cancelled_Date)
            ),
            hideArriveDate: false,
            hideCompany: false,
        },
    ];

    return sections.map((section) => ({
        title: section.title,
        groups: groupBookings(section.bookings, section.hideArriveDate, section.hideCompany, useResidentNotes),
    }));
}

function groupBookings(
    bookings: Booking[],
    hideArriveDate: boolean,
    hideCompany: boolean,
    useResidentNotes: boolean
) {
    const companies = Array.from(new Set(bookings.map((b) => b.Company))).sort();

    return companies.map((company) => {
        const companyBookings = bookings
            .filter((b) => b.Company === company)
            .sort((a, b) =>
                cleanName(`${a.Given} ${a.Surname}`).localeCompare(
                    cleanName(`${b.Given} ${b.Surname}`)
                )
            );

        return {
            company: hideCompany ? "" : company,
            lines: companyBookings.map((booking) => {
                const name = cleanName(`${booking.Given} ${booking.Surname}`);
                const date = hideArriveDate
                    ? ""
                    : `${formatBookingDate(booking.Arrive_Date_Short)} -`;
                const nights = `${booking.Nights} ${
                    booking.Nights > 1 ? "Nights" : "Night"
                }`;
                const repeatText =
                    booking.No_Of_Visits > 0 ? "Repeat resident" : "First time resident";

                const rawNote = useResidentNotes ? booking.Notes : booking.Note1;
                const note = rawNote.match(/{(.*?)}/)?.[1] ?? "";

                return `${name} - ${date} ${nights} - ${booking.BkgSrc} - ${repeatText}, ${note}`;
            }),
        };
    });
}