import type { Metadata } from "next";
import { PT_Sans, JetBrains_Mono } from "next/font/google";
import SiteHeader from "@/app/components/SiteHeader";
import { AuthProvider } from "@/app/providers/AuthProvider";
import "./globals.css";

const ptSans = PT_Sans({
  variable: "--font-pt-sans",
  subsets: ["latin"],
  weight: ["400", "700"],
});

const jetbrainsMono = JetBrains_Mono({
  variable: "--font-jetbrains-mono",
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "DevConnect",
  description: "Developer platform for posts, comments, and collaboration",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" className={`${ptSans.variable} ${jetbrainsMono.variable}`}>
      <body>
        <AuthProvider>
          <SiteHeader />
          {children}
        </AuthProvider>
      </body>
    </html>
  );
}
