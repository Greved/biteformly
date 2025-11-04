"use client";

import { useTheme } from "next-themes";
import { Button } from "@/components/ui/button";
import { useEffect, useState } from "react";

export function ThemeToggle() {
  const { theme, setTheme, resolvedTheme } = useTheme();
  const [mounted, setMounted] = useState(false);
  useEffect(() => setMounted(true), []);
  if (!mounted) return null;

  const isDark = (theme ?? resolvedTheme) === "dark";
  return (
    <Button size="sm" variant="secondary" onClick={() => setTheme(isDark ? "light" : "dark")}
      aria-label="Toggle theme">
      {isDark ? "Light" : "Dark"}
    </Button>
  );
}

