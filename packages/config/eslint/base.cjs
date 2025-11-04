/**
 * Base ESLint config for BiteForm web projects.
 * Projects can extend via: module.exports = { extends: [require.resolve('@biteform/config/eslint/base.cjs')] }
 */
module.exports = {
  root: false,
  env: { browser: true, es2022: true, node: true },
  parser: "@typescript-eslint/parser",
  parserOptions: { ecmaVersion: "latest", sourceType: "module", ecmaFeatures: { jsx: true } },
  settings: { react: { version: "detect" } },
  plugins: ["@typescript-eslint", "react", "react-hooks", "jsx-a11y"],
  extends: [
    "eslint:recommended",
    "plugin:@typescript-eslint/recommended",
    "plugin:react/recommended",
    "plugin:react-hooks/recommended",
    "plugin:jsx-a11y/recommended",
    "prettier"
  ],
  rules: {
    "react/react-in-jsx-scope": "off",
    "react/prop-types": "off"
  }
};

