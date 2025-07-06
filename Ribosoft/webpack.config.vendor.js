const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = (env, argv) => {
  const isDevBuild = argv.mode === 'development';
  const extractCSS = new MiniCssExtractPlugin({
    filename: 'vendor.css',
    chunkFilename: 'vendor.[contenthash:8].css'
  });

  return {
    mode: isDevBuild ? 'development' : 'production',
    stats: {
      modules: false,
      children: false,
      chunks: false,
      chunkModules: false,
      colors: true,
      timings: true
    },
    resolve: {
      extensions: ['.js', '.json'],
      alias: {
        vue$: 'vue/dist/vue.esm-bundler.js'
      },
      fallback: {
        path: false,
        fs: false
      }
    },
    entry: {
      vendor: [
        'bootstrap',
        'bootstrap/dist/css/bootstrap.css',
        'bootstrap-vue-next',
        'event-source-polyfill',
        'jquery',
        'jquery-validation',
        'jquery-validation-unobtrusive',
        'vue',
        'vue-select',
        'vue-select/dist/vue-select.css',
        'qrious',
        'axios',
        '@fortawesome/fontawesome-svg-core',
        '@fortawesome/free-solid-svg-icons'
      ]
    },
    module: {
      rules: [
        {
          test: /\.css$/,
          use: [
            MiniCssExtractPlugin.loader,
            {
              loader: 'css-loader',
              options: {
                sourceMap: isDevBuild,
                importLoaders: 1
              }
            }
          ]
        },
        {
          test: /\.(png|woff|woff2|eot|ttf|svg)$/,
          type: 'asset',
          parser: {
            dataUrlCondition: {
              maxSize: 100000
            }
          },
          generator: {
            filename: 'assets/[name].[hash:8][ext]'
          }
        },
        {
          test: require.resolve('jquery'),
          loader: 'expose-loader',
          options: {
            exposes: ['$', 'jQuery']
          }
        }
      ]
    },
    output: {
      path: path.join(__dirname, 'wwwroot', 'dist'),
      publicPath: 'dist/',
      filename: isDevBuild ? '[name].js' : '[name].[contenthash:8].js',
      library: '[name]_[fullhash]',
      clean: false, // Don't clean vendor files when main build runs
      assetModuleFilename: 'assets/[name].[hash:8][ext]'
    },
    optimization: {
      minimize: !isDevBuild,
      minimizer: [
        new TerserPlugin({
          terserOptions: {
            compress: {
              drop_console: !isDevBuild,
              drop_debugger: !isDevBuild
            },
            format: {
              comments: false
            }
          },
          extractComments: false
        })
      ],
      moduleIds: 'deterministic',
      chunkIds: 'deterministic'
    },
    plugins: [
      extractCSS,
      new webpack.ProvidePlugin({
        $: 'jquery',
        jQuery: 'jquery',
        'window.jQuery': 'jquery'
      }),
      new webpack.DefinePlugin({
        'process.env.NODE_ENV': JSON.stringify(isDevBuild ? 'development' : 'production'),
        __VUE_OPTIONS_API__: JSON.stringify(true),
        __VUE_PROD_DEVTOOLS__: JSON.stringify(isDevBuild),
        __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: JSON.stringify(isDevBuild)
      }),
      new webpack.DllPlugin({
        path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
        name: '[name]_[fullhash]'
      })
    ],
    devtool: isDevBuild ? 'eval-source-map' : 'source-map',
    cache: {
      type: 'filesystem',
      buildDependencies: {
        config: [__filename]
      },
      cacheDirectory: path.resolve(__dirname, '.webpack-cache')
    },
    performance: {
      hints: isDevBuild ? false : 'warning',
      maxEntrypointSize: 1024000,
      maxAssetSize: 1024000
    }
  };
};
