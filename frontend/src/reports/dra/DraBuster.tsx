import { useState } from "react";
import type { DraSection } from "./types";
import { generateDraSections, parseArrivalCSV } from "./draUtils";
import "./dra.css";

export function DraBuster() {
    const [csvFile, setCsvFile] = useState<File | null>(null);
    const [useResidentNotes, setUseResidentNotes] = useState(false);
    const [sections, setSections] = useState<DraSection[]>([]);
    const [isLoading, setIsLoading] = useState(false);

    async function handleGenerate(e: React.FormEvent) {
        e.preventDefault();

        if (!csvFile) {
            alert("Please choose a CSV file.");
            return;
        }

        try {
            setIsLoading(true);

            const bookings = await parseArrivalCSV(csvFile);
            const generatedSections = generateDraSections(
                bookings,
                useResidentNotes
            );

            setSections(generatedSections);
        } catch (error) {
            console.error("DRA failed:", error);
            alert("Failed to parse CSV.");
        } finally {
            setIsLoading(false);
        }
    }

    async function handleCopy() {
        const text = sections
            .map((section) => {
                const groups = section.groups
                    .map((group) => {
                        const company = group.company
                            ? `${group.company}\n`
                            : "";

                        const lines = group.lines
                            .map((line) => `• ${line}`)
                            .join("\n");

                        return `${company}${lines}`;
                    })
                    .join("\n");

                return `${section.title}\n${groups}`;
            })
            .join("\n\n");

        await navigator.clipboard.writeText(text);
        alert("Copied DRA output.");
    }

    return (
        <div id="container">
            <form id="upload-form" onSubmit={handleGenerate}>
                <label htmlFor="csvInput" className="custom-upload">
                    📁 Choose CSV File
                </label>

                <input
                    type="file"
                    id="csvInput"
                    accept=".csv"
                    name="csv"
                    onChange={(e) =>
                        setCsvFile(e.target.files?.[0] ?? null)
                    }
                />

                {csvFile && <p>{csvFile.name}</p>}

                <div className="note-mode">
                    <label>
                        Select where to pull notes from:
                        <input
                            type="radio"
                            name="mode"
                            checked={!useResidentNotes}
                            onChange={() => setUseResidentNotes(false)}
                        />
                        Reservation
                    </label>

                    <label>
                        <input
                            type="radio"
                            name="mode"
                            checked={useResidentNotes}
                            onChange={() => setUseResidentNotes(true)}
                        />
                        Resident
                    </label>
                </div>

                <button type="submit" id="parseArrival" disabled={isLoading}>
                    {isLoading ? "Generating..." : "Generate DRA"}
                </button>

                {sections.length > 0 && (
                    <button
                        type="button"
                        id="copyOutput"
                        onClick={handleCopy}
                    >
                        Copy Output
                    </button>
                )}
            </form>

            {sections.length > 0 && (
                <div id="output">
                    {sections.map((section) => (
                        <div key={section.title}>
                            <p className="title">{section.title}</p>

                            {section.groups.map((group, groupIndex) => (
                                <div key={`${section.title}-${groupIndex}`}>
                                    {group.company && (
                                        <p>
                                            <strong>{group.company}</strong>
                                        </p>
                                    )}

                                    <ul className="booking-list">
                                        {group.lines.map((line, lineIndex) => (
                                            <li key={lineIndex}>{line}</li>
                                        ))}
                                    </ul>
                                </div>
                            ))}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
}