/** @type {import('next').NextConfig} */
const nextConfig = {
    experimental : {
        serverActions : true
    },
    images: {
        remotePatterns : [
            {
                protocol: 'http',
                hostname: 'cdn.pixabay.com'
            },
            {
                protocol: 'https',
                hostname: 'cdn.pixabay.com'
            }
        ]
    }
}

module.exports = nextConfig
