const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');

module.exports = (env, argv) => {
    const isDevBuild = argv.mode === 'development';
    const extractCSS = new MiniCssExtractPlugin({
        filename: 'vendor.css'
    });

    return {
        mode: isDevBuild ? 'development' : 'production',
        stats: { 
            modules: false,
            children: false,
            chunks: false,
            chunkModules: false
        },
        resolve: {
            extensions: ['.js', '.json'],
            alias: {
                'vue$': 'vue/dist/vue.esm-bundler.js'
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
                'qrious',
            ],
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
                                sourceMap: isDevBuild
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
            filename: '[name].js',
            library: '[name]_[fullhash]',
            clean: false // Don't clean vendor files when main build runs
        },
        optimization: {
            minimize: !isDevBuild,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: {
                            drop_console: !isDevBuild
                        }
                    }
                })
            ]
        },
        plugins: [
            extractCSS,
            new webpack.ProvidePlugin({ 
                $: 'jquery', 
                jQuery: 'jquery' 
            }),
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(isDevBuild ? 'development' : 'production'),
                '__VUE_OPTIONS_API__': JSON.stringify(true),
                '__VUE_PROD_DEVTOOLS__': JSON.stringify(false)
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
            }
        }
    };
};
