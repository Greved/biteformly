module.exports = {
  root: true,
  extends: [require.resolve("@biteform/config/eslint/base.cjs")],
  parserOptions: { tsconfigRootDir: __dirname, project: null }
};

