module.exports = {
  css: {
    loaderOptions: {
      postcss: {
        postcssOptions: {
          plugins: [
            require('autoprefixer')
          ]
        }
      }
    }
  },
  outputDir: 'Backend/src/views',
  publicPath: '/views/' // Обновите publicPath, чтобы соответствовать фактическому пути
};
