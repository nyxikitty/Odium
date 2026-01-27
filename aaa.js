const express = require('express');
const fetch = require('node-fetch');
const cors = require('cors');

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());

const PROXY_URLS = [
  'https://node-js-app--nyxikittydev.replit.app',
  'https://avatar-proxy--RyanAnton11.replit.app',
  'https://avatar-proxy--ryanantongiova4.replit.app'
];

function getRandomProxy() {
  return PROXY_URLS[Math.floor(Math.random() * PROXY_URLS.length)];
}

app.get('/api/avatar/search', async (req, res) => {
  try {
    const queryParams = new URLSearchParams();
    if (req.query.query) queryParams.append('query', req.query.query);
    if (req.query.page) queryParams.append('page', req.query.page);
    if (req.query.page_size) queryParams.append('page_size', req.query.page_size);
    
    const proxyUrl = getRandomProxy();
    const targetUrl = `https://api.avtrdb.com/v2/avatar/search?${queryParams.toString()}`;
    
    console.log(`Routing through: ${proxyUrl}`);
    
    const response = await fetch(`${proxyUrl}/proxy?url=${encodeURIComponent(targetUrl)}`, {
      method: 'GET',
      headers: {
        'accept': '*/*',
      }
    });
    
    const data = await response.json();
    res.json(data);
  } catch (error) {
    console.error('Proxy error:', error);
    res.status(500).json({ error: 'Failed to fetch from AVTRDB API', details: error.message });
  }
});

app.get('/api/proxy', async (req, res) => {
  try {
    const targetUrl = req.query.url;
    
    if (!targetUrl) {
      return res.status(400).json({ error: 'Missing url parameter' });
    }
    
    const proxyUrl = getRandomProxy();
    
    console.log(`Routing through: ${proxyUrl} to ${targetUrl}`);
    
    const response = await fetch(`${proxyUrl}/proxy?url=${encodeURIComponent(targetUrl)}`, {
      method: 'GET',
      headers: {
        'accept': '*/*',
      }
    });
    
    const data = await response.json();
    res.json(data);
  } catch (error) {
    console.error('Proxy error:', error);
    res.status(500).json({ error: 'Failed to fetch through proxy', details: error.message });
  }
});

app.get('/', (req, res) => {
  res.json({ 
    status: 'ok', 
    message: 'Rotating Proxy Server',
    proxyCount: PROXY_URLS.length,
    endpoints: {
      search: '/api/avatar/search?query=YOUR_QUERY&page=1&page_size=50',
      generic: '/api/proxy?url=ENCODED_URL'
    }
  });
});

app.get('/api/stats', (req, res) => {
  res.json({
    totalProxies: PROXY_URLS.length,
    proxies: PROXY_URLS
  });
});

app.listen(PORT, () => {
  console.log(`Rotating proxy server running on port ${PORT}`);
  console.log(`Rotating through ${PROXY_URLS.length} proxy URLs`);
});