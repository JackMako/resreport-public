import { useState } from "react";
import { downloadReport } from "../services/reportApi";

export function ClientReportPack() {
    const [yearAFile, setYearAFile] = useState<File | null>(null);
    const [yearBFile, setYearBFile] = useState<File | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    async function handleGenerateReport() {
        if (!yearAFile || !yearBFile) {
            alert("Please upload both CSV files.");
            return;
        }

        const formData = new FormData();
        formData.append("yearAFile", yearAFile);
        formData.append("yearBFile", yearBFile);

        try {
            setIsLoading(true);

            await downloadReport(
                "ClientReportPack",
                formData,
            );
        } catch (error) {
            console.error(error);
            alert("Failed to generate report.");
        } finally {
            setIsLoading(false);
        }
    }

    return (
        <section className="report-card">
            <h2>Client Report Pack</h2>
            <p>Upload Year A and Year B CSV files to generate the YOY report.</p>

            <label>
                Current Calendar Year
                <input
                    type="file"
                    accept=".csv"
                    onChange={(e) => setYearAFile(e.target.files?.[0] ?? null)}
                />
            </label>

            <label>
                Last Calendar Year
                <input
                    type="file"
                    accept=".csv"
                    onChange={(e) => setYearBFile(e.target.files?.[0] ?? null)}
                />
            </label>

            <button onClick={handleGenerateReport} disabled={isLoading}>
                {isLoading ? "Generating..." : "Generate Report"}
            </button>
        </section>
    );
}