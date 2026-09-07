const API_BASE_URL = "https://resreport.onrender.com/api/v1/reports";

export async function downloadReport(
    reportName: string,
    formData: FormData
) {
    const response = await fetch(`${API_BASE_URL}/${reportName}`, {
        method: "POST",
        body: formData,
    });
    console.log(response.headers.get("Content-Disposition"));

    if (!response.ok) {
        throw new Error("Failed to generate report.");
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);

    let fileName = "download.xlsx";

    const contentDisposition = response.headers.get("Content-Disposition");

    console.log("Header:", contentDisposition);

    if (contentDisposition) {
        const parts = contentDisposition.split(";").map(x => x.trim());

        const utf8Part = parts.find(x => x.startsWith("filename*="));
        const normalPart = parts.find(x => x.startsWith("filename="));

        console.log("utf8Part:", utf8Part);
        console.log("normalPart:", normalPart);

        if (utf8Part) {
            fileName = decodeURIComponent(
                utf8Part
                    .replace("filename*=UTF-8''", "")
                    .replace(/^"|"$/g, "")
            );
        } else if (normalPart) {
            fileName = normalPart
                .replace("filename=", "")
                .replace(/^"|"$/g, "");
        }
    }

    console.log("Final filename:", fileName);

    const a = document.createElement("a");
    a.href = url;
    a.setAttribute("download", fileName);
    document.body.appendChild(a);
    a.click();
    a.remove();

    window.URL.revokeObjectURL(url);
}