import "@/styles/globals.css";
import { Inter } from "next/font/google";
import { ThemeProvider } from "@/components/theme-provider";
import { ThemeToggle } from "@/components/theme-toggle";

const inter = Inter({ subsets: ["latin"] });

export const metadata = {
  title: "BiteForm",
  description: "AI form builder"
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={inter.className}>
        <ThemeProvider attribute="class" defaultTheme="system" enableSystem>
          <div className="min-h-dvh bg-background text-foreground">
            <header className="border-b bg-card">
              <div className="container mx-auto px-4 py-3 flex items-center justify-between">
                <div className="font-semibold">BiteForm</div>
                <ThemeToggle />
              </div>
            </header>
            <main className="container mx-auto p-4">{children}</main>
          </div>
        </ThemeProvider>
      </body>
    </html>
  );
}

