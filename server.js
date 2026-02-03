const express = require('express');
const bcrypt = require('bcryptjs');
const jwt = require('jsonwebtoken');
const multer = require('multer');
const cors = require('cors');
const fs = require('fs');
const path = require('path');
require('dotenv').config();

const app = express();

app.use(cors());
app.use(express.json());
app.use('/uploads', express.static('uploads'));
app.use(express.static('public'));

const DATA_DIR = './data';
const USERS_FILE = path.join(DATA_DIR, 'users.json');
const LOCATIONS_FILE = path.join(DATA_DIR, 'locations.json');
const RATINGS_FILE = path.join(DATA_DIR, 'ratings.json');

if (!fs.existsSync(DATA_DIR)) {
    fs.mkdirSync(DATA_DIR);
}

if (!fs.existsSync('uploads')) {
    fs.mkdirSync('uploads');
}

function readJSON(file, defaultValue = []) {
    if (!fs.existsSync(file)) {
        fs.writeFileSync(file, JSON.stringify(defaultValue, null, 2));
        return defaultValue;
    }
    const data = fs.readFileSync(file, 'utf8');
    return JSON.parse(data);
}

function writeJSON(file, data) {
    fs.writeFileSync(file, JSON.stringify(data, null, 2));
}

const storage = multer.diskStorage({
    destination: (req, file, cb) => {
        cb(null, 'uploads/');
    },
    filename: (req, file, cb) => {
        cb(null, Date.now() + '-' + file.originalname);
    }
});

const upload = multer({ 
    storage: storage,
    limits: { fileSize: 5 * 1024 * 1024 },
    fileFilter: (req, file, cb) => {
        if (file.mimetype.startsWith('image/')) {
            cb(null, true);
        } else {
            cb(new Error('Only images are allowed'));
        }
    }
});

const authenticateToken = (req, res, next) => {
    const authHeader = req.headers['authorization'];
    const token = authHeader && authHeader.split(' ')[1];
    
    if (!token) {
        return res.status(401).json({ error: 'Access token required' });
    }
    
    jwt.verify(token, process.env.JWT_SECRET || 'your-secret-key', (err, user) => {
        if (err) {
            return res.status(403).json({ error: 'Invalid token' });
        }
        req.user = user;
        next();
    });
};

const requireModerator = async (req, res, next) => {
    try {
        const users = readJSON(USERS_FILE);
        const user = users.find(u => u.id === req.user.userId);
        if (!user || !user.isModerator) {
            return res.status(403).json({ error: 'Moderator access required' });
        }
        next();
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
};

app.post('/api/auth/signup', upload.single('profilePicture'), async (req, res) => {
    try {
        const { username, email, password } = req.body;
        const users = readJSON(USERS_FILE);
        
        const existingUser = users.find(u => u.email === email || u.username === username);
        if (existingUser) {
            return res.status(400).json({ error: 'User already exists' });
        }
        
        const hashedPassword = await bcrypt.hash(password, 10);
        
        const user = {
            id: Date.now().toString(),
            username,
            email,
            password: hashedPassword,
            profilePicture: req.file ? `/uploads/${req.file.filename}` : null,
            isModerator: false,
            createdAt: new Date().toISOString()
        };
        
        users.push(user);
        writeJSON(USERS_FILE, users);
        
        const token = jwt.sign(
            { userId: user.id }, 
            process.env.JWT_SECRET || 'your-secret-key',
            { expiresIn: '7d' }
        );
        
        res.status(201).json({
            token,
            user: {
                id: user.id,
                username: user.username,
                email: user.email,
                profilePicture: user.profilePicture,
                isModerator: user.isModerator
            }
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error: ' + error.message });
    }
});

app.post('/api/auth/login', async (req, res) => {
    try {
        const { email, password } = req.body;
        const users = readJSON(USERS_FILE);
        
        const user = users.find(u => u.email === email);
        if (!user) {
            return res.status(400).json({ error: 'Invalid credentials' });
        }
        
        const validPassword = await bcrypt.compare(password, user.password);
        if (!validPassword) {
            return res.status(400).json({ error: 'Invalid credentials' });
        }
        
        const token = jwt.sign(
            { userId: user.id }, 
            process.env.JWT_SECRET || 'your-secret-key',
            { expiresIn: '7d' }
        );
        
        res.json({
            token,
            user: {
                id: user.id,
                username: user.username,
                email: user.email,
                profilePicture: user.profilePicture,
                isModerator: user.isModerator
            }
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.get('/api/auth/me', authenticateToken, async (req, res) => {
    try {
        const users = readJSON(USERS_FILE);
        const user = users.find(u => u.id === req.user.userId);
        if (!user) {
            return res.status(404).json({ error: 'User not found' });
        }
        res.json({
            id: user.id,
            username: user.username,
            email: user.email,
            profilePicture: user.profilePicture,
            isModerator: user.isModerator
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.post('/api/locations', authenticateToken, async (req, res) => {
    try {
        const { name, description, lat, lng, hazardLevel } = req.body;
        const locations = readJSON(LOCATIONS_FILE);
        const users = readJSON(USERS_FILE);
        
        const submitter = users.find(u => u.id === req.user.userId);
        
        const location = {
            _id: Date.now().toString(),
            name,
            description,
            lat: parseFloat(lat),
            lng: parseFloat(lng),
            hazardLevel,
            status: 'pending',
            submittedBy: {
                _id: req.user.userId,
                username: submitter.username
            },
            submittedAt: new Date().toISOString(),
            verifiedBy: null,
            verifiedAt: null
        };
        
        locations.push(location);
        writeJSON(LOCATIONS_FILE, locations);
        
        res.status(201).json(location);
    } catch (error) {
        res.status(500).json({ error: 'Server error: ' + error.message });
    }
});

app.get('/api/locations', async (req, res) => {
    try {
        const { status } = req.query;
        let locations = readJSON(LOCATIONS_FILE);
        const ratings = readJSON(RATINGS_FILE);
        
        if (status) {
            locations = locations.filter(loc => loc.status === status);
        }
        
        const locationsWithRatings = locations.map(location => {
            const locationRatings = ratings.filter(r => r.locationId === location._id);
            
            const avgRating = locationRatings.length > 0
                ? locationRatings.reduce((sum, r) => sum + r.rating, 0) / locationRatings.length
                : 0;
            
            return {
                ...location,
                ratings: locationRatings,
                avgRating: avgRating.toFixed(1),
                ratingCount: locationRatings.length
            };
        });
        
        res.json(locationsWithRatings);
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.get('/api/locations/:id', async (req, res) => {
    try {
        const locations = readJSON(LOCATIONS_FILE);
        const ratings = readJSON(RATINGS_FILE);
        const users = readJSON(USERS_FILE);
        
        const location = locations.find(loc => loc._id === req.params.id);
        
        if (!location) {
            return res.status(404).json({ error: 'Location not found' });
        }
        
        const locationRatings = ratings.filter(r => r.locationId === location._id);
        
        const ratingsWithUsers = locationRatings.map(r => {
            const user = users.find(u => u.id === r.userId);
            return {
                userId: r.userId,
                username: user ? user.username : 'Unknown',
                profilePicture: user ? user.profilePicture : null,
                rating: r.rating,
                comment: r.comment,
                createdAt: r.createdAt
            };
        });
        
        const avgRating = locationRatings.length > 0
            ? locationRatings.reduce((sum, r) => sum + r.rating, 0) / locationRatings.length
            : 0;
        
        res.json({
            ...location,
            ratings: ratingsWithUsers,
            avgRating: avgRating.toFixed(1),
            ratingCount: locationRatings.length
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.patch('/api/locations/:id/verify', authenticateToken, requireModerator, async (req, res) => {
    try {
        const locations = readJSON(LOCATIONS_FILE);
        const users = readJSON(USERS_FILE);
        
        const locationIndex = locations.findIndex(loc => loc._id === req.params.id);
        
        if (locationIndex === -1) {
            return res.status(404).json({ error: 'Location not found' });
        }
        
        const moderator = users.find(u => u.id === req.user.userId);
        
        locations[locationIndex].status = 'verified';
        locations[locationIndex].verifiedBy = {
            _id: req.user.userId,
            username: moderator.username
        };
        locations[locationIndex].verifiedAt = new Date().toISOString();
        
        writeJSON(LOCATIONS_FILE, locations);
        
        res.json(locations[locationIndex]);
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.delete('/api/locations/:id', authenticateToken, requireModerator, async (req, res) => {
    try {
        let locations = readJSON(LOCATIONS_FILE);
        let ratings = readJSON(RATINGS_FILE);
        
        const locationIndex = locations.findIndex(loc => loc._id === req.params.id);
        
        if (locationIndex === -1) {
            return res.status(404).json({ error: 'Location not found' });
        }
        
        locations.splice(locationIndex, 1);
        ratings = ratings.filter(r => r.locationId !== req.params.id);
        
        writeJSON(LOCATIONS_FILE, locations);
        writeJSON(RATINGS_FILE, ratings);
        
        res.json({ message: 'Location deleted successfully' });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.post('/api/locations/:id/ratings', authenticateToken, async (req, res) => {
    try {
        const { rating, comment } = req.body;
        const locations = readJSON(LOCATIONS_FILE);
        const ratings = readJSON(RATINGS_FILE);
        const users = readJSON(USERS_FILE);
        
        const location = locations.find(loc => loc._id === req.params.id);
        if (!location) {
            return res.status(404).json({ error: 'Location not found' });
        }
        
        if (location.status !== 'verified') {
            return res.status(400).json({ error: 'Can only rate verified locations' });
        }
        
        const existingRating = ratings.find(r => 
            r.locationId === req.params.id && r.userId === req.user.userId
        );
        
        if (existingRating) {
            return res.status(400).json({ error: 'You have already rated this location' });
        }
        
        const user = users.find(u => u.id === req.user.userId);
        
        const newRating = {
            id: Date.now().toString(),
            locationId: req.params.id,
            userId: req.user.userId,
            rating: parseInt(rating),
            comment: comment || '',
            createdAt: new Date().toISOString()
        };
        
        ratings.push(newRating);
        writeJSON(RATINGS_FILE, ratings);
        
        res.status(201).json({
            userId: user.id,
            username: user.username,
            profilePicture: user.profilePicture,
            rating: newRating.rating,
            comment: newRating.comment,
            createdAt: newRating.createdAt
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error: ' + error.message });
    }
});

app.get('/api/locations/:id/ratings', async (req, res) => {
    try {
        const ratings = readJSON(RATINGS_FILE);
        const users = readJSON(USERS_FILE);
        
        const locationRatings = ratings.filter(r => r.locationId === req.params.id);
        
        const ratingsWithUsers = locationRatings.map(r => {
            const user = users.find(u => u.id === r.userId);
            return {
                userId: r.userId,
                username: user ? user.username : 'Unknown',
                profilePicture: user ? user.profilePicture : null,
                rating: r.rating,
                comment: r.comment,
                createdAt: r.createdAt
            };
        });
        
        res.json(ratingsWithUsers);
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.patch('/api/users/:id/moderator', authenticateToken, requireModerator, async (req, res) => {
    try {
        const { isModerator } = req.body;
        const users = readJSON(USERS_FILE);
        
        const userIndex = users.findIndex(u => u.id === req.params.id);
        if (userIndex === -1) {
            return res.status(404).json({ error: 'User not found' });
        }
        
        users[userIndex].isModerator = isModerator;
        writeJSON(USERS_FILE, users);
        
        res.json({
            id: users[userIndex].id,
            username: users[userIndex].username,
            email: users[userIndex].email,
            profilePicture: users[userIndex].profilePicture,
            isModerator: users[userIndex].isModerator
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

app.get('/api/stats', async (req, res) => {
    try {
        const locations = readJSON(LOCATIONS_FILE);
        const users = readJSON(USERS_FILE);
        const ratings = readJSON(RATINGS_FILE);
        
        const totalLocations = locations.length;
        const verifiedLocations = locations.filter(l => l.status === 'verified').length;
        const pendingLocations = locations.filter(l => l.status === 'pending').length;
        const totalUsers = users.length;
        const totalRatings = ratings.length;
        
        res.json({
            totalLocations,
            verifiedLocations,
            pendingLocations,
            totalUsers,
            totalRatings
        });
    } catch (error) {
        res.status(500).json({ error: 'Server error' });
    }
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`RuinFinder backend running on port ${PORT}`);
    console.log(`Using JSON file storage in ./data directory`);
});