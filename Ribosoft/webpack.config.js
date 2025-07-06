const path = require('path');
const webpack = require('webpack');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require('terser-webpack-plugin');
const { VueLoaderPlugin } = require('vue-loader');

const bundleOutputDir = './wwwroot/dist';

module.exports = (env, argv) => {
    const isDevBuild = argv.mode === 'development';

    return {
        mode: isDevBuild ? 'development' : 'production',
        stats: { 
            modules: false,
            children: false,
            chunks: false,
            chunkModules: false
        },
        context: __dirname,
        resolve: {
            extensions: ['.js', '.vue', '.ts', '.json'],
            alias: {
                'vue$': 'vue/dist/vue.esm-bundler.js',
                '@': path.resolve(__dirname, 'ClientApp'),
                '~': path.resolve(__dirname, 'node_modules')
            }
        },
        entry: {
            'main': './ClientApp/boot.js',
            'request': './ClientApp/Request/request.js',
            'details': './ClientApp/Jobs/details.js'
        },
        module: {
            rules: [
                {
                    test: /\.vue$/,
                    loader: 'vue-loader',
                    include: path.resolve(__dirname, 'ClientApp')
                },
                {
                    test: /\.vue\.html$/,
                    loader: 'vue-loader',
                    include: path.resolve(__dirname, 'ClientApp')
                },
                {
                    test: /\.css$/,
                    use: [
                        isDevBuild ? 'style-loader' : MiniCssExtractPlugin.loader,
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: isDevBuild
                            }
                        }
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|eot|ttf)$/,
                    type: 'asset',
                    parser: {
                        dataUrlCondition: {
                            maxSize: 25000
                        }
                    }
                }
            ]
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: isDevBuild ? '[name].js' : '[name].[contenthash:8].js',
            publicPath: '../dist/',
            clean: true
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
            ],
            splitChunks: {
                chunks: 'all',
                cacheGroups: {
                    vendor: {
                        test: /[\\/]node_modules[\\/]/,
                        name: 'vendors',
                        chunks: 'all'
                    }
                }
            }
        },
        plugins: [
            new VueLoaderPlugin(),
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(isDevBuild ? 'development' : 'production'),
                '__VUE_OPTIONS_API__': JSON.stringify(true),
                '__VUE_PROD_DEVTOOLS__': JSON.stringify(false)
            }),
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            ...(isDevBuild ? [] : [
                new MiniCssExtractPlugin({
                    filename: '[name].[contenthash:8].css'
                })
            ])
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
