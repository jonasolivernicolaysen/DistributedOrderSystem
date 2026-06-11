import { BrowserRouter, Routes, Route } from "react-router-dom";

import RegisterPage from "./pages/RegisterPage";
import LoginPage from "./pages/LoginPage";
import ProductsPage from "./pages/ProductsPage";
import CartPage from "./pages/CartPage";

import Navbar from "./components/Navbar";

function App() {
    return (
        <BrowserRouter>
        <Navbar />
            <Routes>
                <Route path="/" element={<LoginPage />} />
                <Route path="/register" element={<RegisterPage />} />
                <Route path="/login" element={<LoginPage />} />
                <Route path="/products" element={<ProductsPage />} />
                <Route path="/cart" element={<CartPage />} />
            </Routes>
        </BrowserRouter>
    )
}

export default App;
