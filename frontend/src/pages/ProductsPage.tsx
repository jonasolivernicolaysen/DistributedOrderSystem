import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";

interface Product {
    productId: string;
    name: string;
    description: string;
    price: number;
}

function ProductsPage() {
    const [products, setProducts] = useState<Product[]>([]);
    const [expandedProduct, setExpandedProduct] = useState<string | null>(null);
    const [quantities, setQuantities] = useState<Record<string, number>>({});
    const [isAdding, setIsAdding] = useState(false);

    const navigate = useNavigate();

    const addProductToCart = async (productId: string, quantity: number) => {
        if (isAdding) return;
        setIsAdding(true);

        try {
            const token = localStorage.getItem("token");
            const response = await fetch("https://localhost:7144/api/orders/cart", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({ productId, quantity })
            });

            if (!response.ok) {
                const error = await response.json();
                alert(error.error);
                return;
            }

            alert("Product added to cart");
        } catch (error) {
            console.error(error);
            alert("Could not connect to the server");
        } finally {
            setIsAdding(false);
        }
    };

    useEffect(() => {
        const showProducts = async () => {
            try {
                const token = localStorage.getItem("token");
                const response = await fetch("https://localhost:7144/api/products", {
                    method: "GET",
                    headers: {
                        "Content-Type": "application/json",
                        "Authorization": `Bearer ${token}`
                    }
                });

                if (!response.ok) {
                    alert(`Request failed: ${response.status}`);
                    return;
                }

                const data = await response.json();
                setProducts(data);
            } catch (error) {
                console.error(error);
                alert("Could not connect to the server");
            }
        };

        showProducts();
    }, []);

    return (
        <div className="container mt-4 pb-5">

            {/* Header */}
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="mb-0">Products</h4>
                <div className="d-flex gap-2">
                    <button
                        className="btn btn-outline-primary btn-sm"
                        onClick={() => navigate("/cart")}
                    >
                        Cart →
                    </button>
                    <button
                        className="btn btn-primary btn-sm"
                        onClick={() => navigate("/products/create")}
                    >
                        + Add Listing
                    </button>
                </div>
            </div>

            {/* Grid */}
            <div className="row align-items-start">
                {products.length === 0 && (
                    <p className="text-muted">No products available.</p>
                )}

                {products.map((product: Product) => {
                    const isExpanded = expandedProduct === product.productId;

                    return (
                        <div key={product.productId} className="col-md-4 mb-3">
                            <div
                                className={`card shadow-sm ${isExpanded ? "border-primary" : ""}`}
                                style={{ cursor: "pointer" }}
                                onClick={() => setExpandedProduct(isExpanded ? null : product.productId)}
                            >
                                <div className="card-body">
                                    <h6 className="card-title mb-1">{product.name}</h6>
                                    <span className="text-muted small">${product.price.toFixed(2)}</span>
                                </div>

                                {isExpanded && (
                                    <div className="card-body border-top pt-3" onClick={(e) => e.stopPropagation()}>
                                        <p className="text-muted small mb-3">{product.description}</p>

                                        <div className="d-flex gap-2">
                                            <input
                                                type="number"
                                                className="form-control form-control-sm"
                                                style={{ width: "70px" }}
                                                min={1}
                                                value={quantities[product.productId] ?? 1}
                                                onChange={(e) => setQuantities({
                                                    ...quantities,
                                                    [product.productId]: Number(e.target.value)
                                                })}
                                            />
                                            <button
                                                className="btn btn-primary btn-sm flex-grow-1"
                                                disabled={isAdding}
                                                onClick={() => addProductToCart(
                                                    product.productId,
                                                    quantities[product.productId] ?? 1
                                                )}
                                            >
                                                {isAdding ? "Adding…" : "Add to cart"}
                                            </button>
                                        </div>
                                    </div>
                                )}
                            </div>
                        </div>
                    );
                })}
            </div>
        </div>
    );
}

export default ProductsPage;